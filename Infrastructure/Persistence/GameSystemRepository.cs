namespace Infrastructure.Persistence;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Persistence;
using Microsoft.EntityFrameworkCore;

public class GameSystemRepository(DatabaseContext context) : IGameSystemRepository {

  public async Task<GameSystem?> GetByIdAsync(Guid id, Guid? ownerId = null) {
    return await context.GameSystems.Include(g => g.Chapters)
                  .Where(g => g.OwnerId == null || g.OwnerId == ownerId)
                  .FirstOrDefaultAsync(g => g.Id == id);
  }

  public async Task<CursorResult<GameSystem>> BrowseAsync(
    int pageSize,
    string? searchPhrase = null,
    Guid? cursor = null, 
    Guid? ownerId = null) {

    IQueryable<GameSystem> query = context.GameSystems
                                          .Where(g => g.OwnerId == null || g.OwnerId == ownerId)
                                          .Where(g => searchPhrase == null || EF.Functions.ILike(g.Title, $"%{searchPhrase}%"))
                                          .OrderByDescending(g => g.Id);
    
    if (cursor is not null) {
      query = query.Where(g => g.Id < cursor);
    }

    List<GameSystem> items = await query.Take(pageSize + 1).ToListAsync();

    bool hasMoreItems = items.Count > pageSize;
    Guid? nextCursor = null;

    if (hasMoreItems) {
      items.RemoveAt(items.Count - 1);
      nextCursor = items.LastOrDefault()?.Id;
    }
    
    return new CursorResult<GameSystem>(items, nextCursor, hasMoreItems);
  }
  
  public void Add(GameSystem gameSystem) {
    context.GameSystems.Add(gameSystem);
  }
}
