namespace Domain.Ports.Persistence;

using Dtos;
using Entities;

public interface ISessionRepository {
  Task<Session?> GetByIdAsync(Guid campaignId, Guid id, Guid ownerId);
  Task<CursorResult<Session>> BrowseAsync(Guid campaignId, Guid ownerId, int pageSize, Guid? cursor = null);

  Task<Scene?> GetCurrentSceneAsync(Guid id, Guid ownerId);
  
  void Add(Session session);

}
