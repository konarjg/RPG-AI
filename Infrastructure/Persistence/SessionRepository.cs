namespace Infrastructure.Persistence;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Persistence;
using Microsoft.EntityFrameworkCore;

public class SessionRepository(DatabaseContext context) : ISessionRepository {

  public async Task<Session?> GetByIdAsync(Guid campaignId,
    Guid id,
    Guid ownerId) {

    return await context.Sessions.FirstOrDefaultAsync(s => s.Id == id
                                                           && s.CampaignId == campaignId 
                                                           && s.Campaign.OwnerId == ownerId);
  }
  public async Task<CursorResult<Session>> BrowseAsync(Guid campaignId,
    Guid ownerId,
    int pageSize,
    Guid? cursor = null) {
    
    IQueryable<Session> query = context.Sessions
                                        .Where(c => c.CampaignId == campaignId && c.Campaign.OwnerId == ownerId)
                                        .OrderByDescending(c => c.Id);

    if (cursor is not null)
    {
      query = query.Where(c => c.Id < cursor);
    }

    List<Session> items = await query.Take(pageSize + 1).ToListAsync();

    bool hasMoreItems = items.Count > pageSize;
    Guid? nextCursor = null;

    if (hasMoreItems)
    {
      items.RemoveAt(items.Count - 1);
      nextCursor = items.LastOrDefault()?.Id;
    }

    return new CursorResult<Session>(items, nextCursor, hasMoreItems);
  }
  
  public async Task<Scene?> GetCurrentSceneAsync(Guid id,
    Guid ownerId) {

    return await context.Sessions.Where(s => s.Id == id && s.Campaign.OwnerId == ownerId)
                        .Select(s => s.CurrentScene)
                        .FirstOrDefaultAsync();
  }

  public async Task<Scene?> GetSceneByIdAsync(Guid campaignId,
    Guid ownerId,
    Guid sessionId,
    Guid id) {

    throw new NotImplementedException();
  }
  
  public Task<CursorResult<Scene>> BrowseScenesAsync(Guid campaignId,
    Guid ownerId,
    Guid sessionId,
    int pageSize,
    Guid? cursor = null) => throw new NotImplementedException();
  
  public void Add(Session session) {
    context.Sessions.Add(session);
  }
  
  public void AddScene(Scene scene) {
    throw new NotImplementedException();
  }
}
