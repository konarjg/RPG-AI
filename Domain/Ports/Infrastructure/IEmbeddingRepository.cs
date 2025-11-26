using Domain.Entities;

namespace Domain.Ports.Infrastructure;

public interface IEmbeddingRepository {
  Task<List<GameRuleChunk>> FindRelevantRulesAsync(
    Guid gameSystemId, 
    float[] queryEmbedding, 
    int limit = 5, 
    CancellationToken cancellationToken = default);
  
  Task<List<CampaignEvent>> FindRelevantHistoryAsync(
    Guid campaignId, 
    float[] queryEmbedding, 
    int limit = 10, 
    CancellationToken cancellationToken = default);
  
  Task<List<Session>> FindRelevantSessionSummariesAsync(
    Guid campaignId,
    float[] queryEmbedding,
    int limit = 3,
    CancellationToken cancellationToken = default);
}
