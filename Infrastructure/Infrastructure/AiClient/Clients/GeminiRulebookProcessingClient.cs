namespace Infrastructure.Infrastructure.AiClient.Clients;

using System.Text;
using Application.Exceptions;
using Domain.Entities;
using Domain.Ports.Infrastructure.Dtos;
using Google.GenAI.Types;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Options;
using Util;
using File = System.IO.File;
using GeminiClient = Google.GenAI.Client;
using System.Diagnostics;
using global::Infrastructure.Diagnostics;
using GeminiFile = Google.GenAI.Types.File;

public class GeminiRulebookProcessingClient : IRulebookProcessingClient
{
  private AiClientOptions _options;
  private readonly GeminiClient _geminiClient;
  private readonly Lazy<Task<string>> _rulebookPrompt;

  public GeminiRulebookProcessingClient(IOptions<AiClientOptions> options)
  {
    _options = options.Value;
    _rulebookPrompt = new Lazy<Task<string>>(
      File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, _options.RulebookAgent.Prompt))
    );

    _geminiClient = new GeminiClient(apiKey: _options.RulebookAgent.ApiKey);
  }

  public async Task<AiSplitRulebookResponse> SplitRulebookAsync(AiSplitRulebookRequest request)
  {
    using Activity? activity = RpgAiActivitySource.Instance.StartActivity("SplitRulebook");
    activity?.SetTag("gen_ai.system", "gemini");
    activity?.SetTag("gen_ai.request.model", _options.RulebookAgent.Model);

    GeminiFile file = await _geminiClient.Files.UploadAsync(request.RulebookStream, request.RulebookStream.Length, mimeType: request.RulebookContentType);

    activity?.SetTag("gen_ai.prompt", (await _rulebookPrompt.Value).Replace("{0}", _options.EmbeddingAgent.Dimensions.ToString()));

    GenerateContentConfig config = new()
    {
      ResponseSchema = GeminiJsonSchemaGenerator.FromType<AiSplitRulebookResponse>(),
      ResponseMimeType = "application/json"
    };

    Content content = new()
    {
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

    try
    {
      GenerateContentResponse generateContentResponse = await _geminiClient.Models.GenerateContentAsync(_options.RulebookAgent.Model, content, config);
      string responseJson = generateContentResponse.Candidates?
                              .FirstOrDefault()?.Content?.Parts?
                              .FirstOrDefault()?.Text ?? throw new RulebookSplitException("Empty response.");

      activity?.SetTag("gen_ai.response", responseJson);

      AiSplitRulebookResponse response = JsonConvert.DeserializeObject<AiSplitRulebookResponse>(responseJson)
                                         ?? throw new RulebookSplitException("Invalid response format.");

      return response;
    }
    catch (Exception ex)
    {
      activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
      activity?.AddException(ex);
      throw;
    }
  }
}
