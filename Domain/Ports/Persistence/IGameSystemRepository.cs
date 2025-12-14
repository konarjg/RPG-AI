namespace Domain.Ports.Persistence;

using Dtos;
using Entities;

public interface IGameSystemRepository {
  Task<GameSystem?> GetByIdAsync(Guid id, Guid? ownerId = null);
  Task<CursorResult<GameSystem>> BrowseAsync(string searchPhrase, int pageSize, Guid? cursor = null, Guid? ownerId = null);
  void Add(GameSystem gameSystem);
}
