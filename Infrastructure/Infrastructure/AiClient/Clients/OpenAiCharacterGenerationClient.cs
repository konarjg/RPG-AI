namespace Infrastructure.Infrastructure.AiClient.Clients;

using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics.CodeAnalysis;
using Domain.Exceptions;
using Domain.Ports.Infrastructure.Dtos;
using Google.GenAI.Types;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using OpenAI;
using OpenAI.Chat;
using Options;
using Util;
using File = System.IO.File;
using JsonSchema = NJsonSchema.JsonSchema;
using System.Diagnostics;
using Application.Exceptions;
using global::Infrastructure.Diagnostics;

public class OpenAiCharacterGenerationClient : ICharacterGenerationClient {

  private readonly AiClientOptions _options;
  private readonly OpenAIClient _characterGenerationClient;
  private readonly Lazy<Task<string>> _characterGenerationPrompt;
  private readonly ILogger<OpenAiCharacterGenerationClient> _logger;

  public OpenAiCharacterGenerationClient(ILogger<OpenAiCharacterGenerationClient> logger, 
                                         IOptions<AiClientOptions> options,
                                         IHttpClientFactory httpClientFactory) {
    _options = options.Value;
    _logger = logger;
    
    _characterGenerationPrompt = new Lazy<Task<string>>(
      File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,_options.CharacterGenerationAgent.Prompt))
    );
    
    _characterGenerationClient = new OpenAIClient(new ApiKeyCredential(_options.CharacterGenerationAgent.ApiKey),new OpenAIClientOptions() {
      Endpoint = new Uri(_options.CharacterGenerationAgent.BaseUrl),
      Transport = new HttpClientPipelineTransport(httpClientFactory.CreateClient("GenerativeAi"))
    });
  }
  
  [Experimental("OPENAI001")]
  public async Task<AiGenerateCharacterResponse> GenerateCharacterAsync(AiGenerateCharacterRequest request) {
    string[] promptParts = (await _characterGenerationPrompt.Value)
      .Replace("<CampaignOverview>", request.CampaignOverview)
      .Replace("<CharacterConcept>", request.Concept is not null ? "### Character concept\n" + request.Concept : "")
      .Replace("<CharacterCreationGuide>", request.CharacterCreationGuide)
      .Replace("<CharacterSheetClassHierarchy>", request.CharacterSheetClassHierarchy)
      .Split("<User>");

    (string systemPrompt, string userPrompt) = (promptParts[0], promptParts[1]);

    List<ChatMessage> prompt = new() {
      new SystemChatMessage(systemPrompt),
      new UserChatMessage(userPrompt)
    };

    string schemaJson = JsonConvert.SerializeObject(JsonSchema.FromType<AiGenerateCharacterResponse>());

    ChatCompletionOptions options = new() {
      ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
        "character_generation_response",
        BinaryData.FromString(schemaJson),
        jsonSchemaIsStrict: true
      ),
      ReasoningEffortLevel = ChatReasoningEffortLevel.High
    };

    ChatClient chatClient = _characterGenerationClient.GetChatClient(_options.CharacterGenerationAgent.Model);
    
    using Activity? activity = RpgAiActivitySource.Instance.StartActivity("GenerateCharacter");
    activity?.SetTag("gen_ai.system", "openai");
    activity?.SetTag("gen_ai.request.model", _options.CharacterGenerationAgent.Model);
    activity?.SetTag("gen_ai.prompt", userPrompt);
    activity?.SetTag("gen_ai.system_prompt", systemPrompt);

    using (_logger.BeginScope(new Dictionary<string, object> { ["CharacterConcept"] = request.Concept ?? "None" })) {
        _logger.LogInformation("Starting character generation.");
        
        try {
            ClientResult<ChatCompletion> completion = await chatClient.CompleteChatAsync(prompt, options);

            string rawResponse = completion.GetRawResponse().Content.ToString() 
                                 ?? throw new CharacterGenerationException("Could not generate a character. No response available.");
            
            activity?.SetTag("gen_ai.response", rawResponse);
    
            AiStructuredResponse structuredResponse = JsonConvert.DeserializeObject<AiStructuredResponse>(rawResponse) 
                                                      ?? throw new CharacterGenerationException("Could not generate a character. Invalid response format.");

            string responseRaw = completion.Value.Content?[0]?.Text
                                 ?? throw new CharacterGenerationException("Could not generate a character. Invalid response format.");
    
            AiGenerateCharacterResponse response = JsonConvert.DeserializeObject<AiGenerateCharacterResponse>(responseRaw)
                   ?? throw new CharacterGenerationException("Could not generate a character. Invalid response format.");

            string? reasoning = structuredResponse.Choices.First().Message.Reasoning;
            activity?.SetTag("gen_ai.reasoning", reasoning);
            
            _logger.LogInformation("Character generation completed. Reasoning: {Reasoning}", reasoning ?? "[NO REASONING FOUND]");
            return response;
        } catch (Exception ex) {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _logger.LogError(ex, "Character generation failed.");
            throw;
        }
    }
  }
}
