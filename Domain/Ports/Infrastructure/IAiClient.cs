using Domain.Ports.Infrastructure.Dtos;

namespace Domain.Ports.Infrastructure;

using Entities;

public interface IAiClient {
  Task<AiCreateCharacterResponse> CreateCharacterAsync(AiCreateCharacterRequest createCharacterRequest, CancellationToken cancellationToken = default);
  Task<AiRulebookChunkResponse> ChunkRulebookAsync(Stream rulebookStream,
    CancellationToken cancellationToken = default);
  Task<float[]> GenerateRetrievalQueryEmbeddingAsync(string currentEvent, List<CampaignEvent> recentEvents, CancellationToken cancellationToken = default);
  Task<string> SummarizeSessionAsync(Session session, CancellationToken cancellationToken = default);
  Task<AiActionResponse> ReactToEventAsync(AiActionRequest actionRequest, CancellationToken cancellationToken = default);
  Task<float[]> EmbedTextAsync(string text, CancellationToken cancellationToken = default);
}
