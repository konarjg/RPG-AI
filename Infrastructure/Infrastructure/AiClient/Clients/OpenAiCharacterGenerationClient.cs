namespace Infrastructure.Infrastructure.AiClient.Clients;

using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using Application.Exceptions;
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

public class OpenAiCharacterGenerationClient : ICharacterGenerationClient {

  private readonly AiClientOptions _options;
  private readonly OpenAIClient _characterGenerationClient;
  private readonly Lazy<Task<string>> _characterGenerationPrompt;
  private readonly ILogger<OpenAiCharacterGenerationClient> _logger;

  public OpenAiCharacterGenerationClient(ILogger<OpenAiCharacterGenerationClient> logger, IOptions<AiClientOptions> options) {
    _options = options.Value;
    _logger = logger;
    
    _characterGenerationPrompt = new Lazy<Task<string>>(
      File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,_options.CharacterGenerationAgent.Prompt))
    );
    
    _characterGenerationClient = new OpenAIClient(new ApiKeyCredential(_options.CharacterGenerationAgent.ApiKey),new OpenAIClientOptions() {
      Endpoint = new Uri(_options.CharacterGenerationAgent.BaseUrl)
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
    ClientResult<ChatCompletion> completion = await chatClient.CompleteChatAsync(prompt, options);
    
    string rawResponse = completion.GetRawResponse().Content.ToString() 
                         ?? throw new CharacterGenerationException("Could not generate a character. No response available.");
    
    
    AiStructuredResponse structuredResponse = JsonConvert.DeserializeObject<AiStructuredResponse>(rawResponse) 
                                              ?? throw new CharacterGenerationException("Could not generate a character. Invalid response format.");

    string responseRaw = completion.Value.Content?[0]?.Text
                         ?? throw new CharacterGenerationException("Could not generate a character. Invalid response format.");
    
    AiGenerateCharacterResponse response = JsonConvert.DeserializeObject<AiGenerateCharacterResponse>(responseRaw)
           ?? throw new CharacterGenerationException("Could not generate a character. Invalid response format.");

    _logger.LogInformation(structuredResponse.Choices.First().Message.Reasoning ?? "[NO REASONING FOUND]");
    
    return response;
  }
}
