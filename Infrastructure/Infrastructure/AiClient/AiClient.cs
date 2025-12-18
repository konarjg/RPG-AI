namespace Infrastructure.Infrastructure.AiClient;

using System.Threading.Tasks;
using Clients.Interfaces;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;

public class AiClient(IRulebookProcessingClient rulebookProcessingClient, IEmbeddingClient embeddingClient, ICharacterGenerationClient characterGenerationClient) : IAiClient {
  
  public async Task<AiSplitRulebookResponse> SplitRulebookAsync(AiSplitRulebookRequest request) {
    return await rulebookProcessingClient.SplitRulebookAsync(request);
  }
  public async Task<AiEmbedTextResponse> EmbedTextAsync(AiEmbedTextRequest request) {
    return await embeddingClient.EmbedTextAsync(request);
  }

  public async Task<List<AiEmbedTextResponse>> EmbedAllTextsAsync(List<AiEmbedTextRequest> requests) {
    return await embeddingClient.EmbedAllTextsAsync(requests);
  }

  public async Task<AiGenerateCharacterResponse> GenerateCharacterAsync(AiGenerateCharacterRequest request) {
    return await characterGenerationClient.GenerateCharacterAsync(request);
  }
}