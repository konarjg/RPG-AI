namespace Domain.Ports.Persistence;

using Dtos;
using Entities;

public interface ISessionRepository {
  Task<Session?> GetByIdAsync(Guid campaignId, Guid id, Guid ownerId);
  Task<CursorResult<Session>> BrowseAsync(Guid campaignId, Guid ownerId, int pageSize, Guid? cursor = null);

  Task<Scene?> GetCurrentSceneAsync(Guid id, Guid ownerId);
  
  Task<Scene?> GetSceneByIdAsync(
    Guid campaignId,
    Guid ownerId,
    Guid sessionId,
    Guid id);

  Task<CursorResult<Scene>> BrowseScenesAsync(
    Guid campaignId,
    Guid ownerId,
    Guid sessionId,
    int pageSize,
    Guid? cursor = null);
  
  void Add(Session session);
  void AddScene(Scene scene);
}
