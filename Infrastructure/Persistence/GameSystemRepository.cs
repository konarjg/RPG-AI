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

  public async Task<CursorResult<GameSystem>> BrowseAsync(string searchPhrase,
    int pageSize,
    Guid? cursor = null, Guid? ownerId = null) {

    IQueryable<GameSystem> query = context.GameSystems
                                          .Include(g => g.Chapters)
                                          .Where(g => g.OwnerId == null || g.OwnerId == ownerId)
                                          .Where(g => g.Title.Contains(searchPhrase))
                                          .OrderByDescending(g => g.Id);

    if (cursor is not null) {
      query = query.Where(g => g.Id < cursor);
    }

    List<GameSystem> items = await query.Take(pageSize + 1).ToListAsync();

    bool hasMoreItems = items.Count > pageSize;

    if (hasMoreItems) {
      items.RemoveAt(items.Count - 1);
    }
    
    return new CursorResult<GameSystem>(items, items.LastOrDefault()?.Id, hasMoreItems);
  }
  
  public void Add(GameSystem gameSystem) {
    context.GameSystems.Add(gameSystem);
  }
}
