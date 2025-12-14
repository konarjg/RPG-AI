namespace Infrastructure.Infrastructure;

using System.ClientModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Exceptions;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Options;
using Environment = System.Environment;
using File = System.IO.File;
using GeminiClient = Google.GenAI.Client;
using GeminiFile = Google.GenAI.Types.File;

public class AiClient : IAiClient {
  private readonly AiClientOptions _options;
  private readonly Lazy<Task<string>> _rulebookPrompt;
  private readonly ILogger<AiClient> _logger;

  private readonly GeminiClient _geminiClient;
  private readonly OpenAIClient _embeddingClient;
  
  public AiClient(IOptions<AiClientOptions> options, ILogger<AiClient> logger) {
    _options = options.Value;
    _rulebookPrompt = new Lazy<Task<string>>(
      File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,_options.RulebookAgent.Prompt))
    );
      
    _logger = logger;
    _geminiClient = new GeminiClient(apiKey: _options.RulebookAgent.ApiKey);
    _embeddingClient = new OpenAIClient(new ApiKeyCredential(_options.EmbeddingAgent.ApiKey),new OpenAIClientOptions() {
      Endpoint = new Uri(_options.EmbeddingAgent.BaseUrl)
    });
  }

  public async Task<AiSplitRulebookResponse> SplitRulebookAsync(AiSplitRulebookRequest request) {
    GeminiFile file = await _geminiClient.Files.UploadAsync(request.RulebookStream, request.RulebookStream.Length, mimeType: request.RulebookContentType);
    
    GenerateContentConfig config = new() {
      ResponseSchema = Schema.FromJson(GetCleanGeminiSchema()),
      ResponseMimeType = "application/json"
    };

    Content content = new() {
      Parts = new List<Part>() {
        new Part() {
          FileData = new FileData() {
            FileUri = file.Uri,
            MimeType = file.MimeType
          }
        },
        new Part() {
          Text = (await _rulebookPrompt.Value).Replace("{0}", _options.EmbeddingAgent.Dimensions.ToString()) 
        }
      }
    };

    GenerateContentResponse generateContentResponse = await _geminiClient.Models.GenerateContentAsync(_options.RulebookAgent.Model,content,config);
    string responseJson = generateContentResponse.Candidates?
                            .FirstOrDefault()?.Content?.Parts?
                            .FirstOrDefault()?.Text ?? throw new RulebookSplitException("Empty response.");

    AiSplitRulebookResponse response = JsonConvert.DeserializeObject<AiSplitRulebookResponse>(responseJson)
                                       ?? throw new RulebookSplitException("Invalid response format.");

    return response;
  }
  
  public async Task<AiEmbedTextResponse> EmbedTextAsync(AiEmbedTextRequest request) {
    EmbeddingClient embeddingClient = _embeddingClient.GetEmbeddingClient(_options.EmbeddingAgent.Model);
    ClientResult<OpenAIEmbedding> embeddings = await embeddingClient.GenerateEmbeddingAsync(request.Text);

    return new AiEmbedTextResponse(embeddings.Value.ToFloats().ToArray());
  }

  public async Task<List<AiEmbedTextResponse>> EmbedAllTextsAsync(List<AiEmbedTextRequest> requests) {
    if (requests.Count == 0) {
      return new List<AiEmbedTextResponse>();
    }
    
    EmbeddingClient embeddingClient = _embeddingClient.GetEmbeddingClient(_options.EmbeddingAgent.Model);
    List<string> inputs = requests.Select(r => r.Text).ToList();
    
    ClientResult<OpenAIEmbeddingCollection> embeddings = await embeddingClient.GenerateEmbeddingsAsync(inputs);
    
    return embeddings.Value.Select(v => new AiEmbedTextResponse(v.ToFloats().ToArray())).ToList();
  }
  
  private string GetCleanGeminiSchema() {
    JsonSchema schema = JsonSchema.FromType<AiSplitRulebookResponse>();
    string rawJson = schema.ToJson();
    
    JObject root = JObject.Parse(rawJson);
    
    CleanSchemaToken(root);

    return root.ToString();
  }

  private void CleanSchemaToken(JToken token) {
    if (token is not JObject obj) {
      if (token is not JArray array) {
        return;
      }
    
      foreach (JToken item in array) {
        CleanSchemaToken(item);
      }

      return;
    }
    
    obj.Remove("additionalProperties");
    obj.Remove("$schema");
    obj.Remove("title"); 
    obj.Remove("x-enumNames"); 
      
    foreach (JProperty property in obj.Properties().ToList()) {
      CleanSchemaToken(property.Value);
    }
  }
}