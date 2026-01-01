namespace Domain.Ports.Persistence;

using Dtos;
using Entities;

public interface ISceneRepository {
  
  Task<Scene?> GetSceneWithMessagesByIdAsync(
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

  Task<Message?> GetMessageByIdAsync(Guid campaignId,
    Guid sessionId,
    Guid ownerId,
    Guid id,
    Guid messageId);

  Task<CursorResult<Message>> BrowseMessagesAsync(Guid campaignId,
    Guid sessionId,
    Guid ownerId,
    int pageSize,
    Guid? cursor = null);
  
  void Add(Scene scene);
}
