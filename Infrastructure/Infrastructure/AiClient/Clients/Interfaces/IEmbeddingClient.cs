namespace Infrastructure.Infrastructure.AiClient.Clients.Interfaces;

using Domain.Ports.Infrastructure.Dtos;

public interface IEmbeddingClient {
  Task<AiEmbedTextResponse> EmbedTextAsync(AiEmbedTextRequest request);
  Task<List<AiEmbedTextResponse>> EmbedAllTextsAsync(List<AiEmbedTextRequest> requests);
}
