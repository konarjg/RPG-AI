namespace Infrastructure.Infrastructure.AiClient.Clients;

using System.ClientModel;
using Domain.Ports.Infrastructure.Dtos;
using Interfaces;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using Options;

public class OpenAiEmbeddingClient : IEmbeddingClient {
  private AiClientOptions _options;
  private readonly OpenAIClient _embeddingClient;
  
  public OpenAiEmbeddingClient(IOptions<AiClientOptions> options) {
    _options = options.Value;
    
    _embeddingClient = new OpenAIClient(new ApiKeyCredential(_options.EmbeddingAgent.ApiKey),new OpenAIClientOptions() {
      Endpoint = new Uri(_options.EmbeddingAgent.BaseUrl)
    });
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
}
