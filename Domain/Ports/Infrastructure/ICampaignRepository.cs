namespace Domain.Ports.Infrastructure;

using Dtos;
using Entities;

public interface ICampaignRepository {
  Task<Campaign?> GetByIdAsync(Guid id,
    Guid userId,
    CancellationToken cancellationToken = default);

  Task<List<Campaign>> GetAllAsync(Guid userId,
    CancellationToken cancellationToken = default);
  
  Task<CursorResult<CampaignEvent>> GetEventsAsync(
    Guid campaignId,
    Guid sessionId,
    Guid userId,
    string? cursor, 
    int pageSize,
    CancellationToken cancellationToken = default);
  
  void Add(Campaign campaign);
}
