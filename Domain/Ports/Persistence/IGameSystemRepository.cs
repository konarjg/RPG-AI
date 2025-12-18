namespace Domain.Ports.Persistence;

using Dtos;
using Entities;

public interface IGameSystemRepository {
  Task<GameSystem?> GetByIdAsync(Guid id, Guid? ownerId = null);
  Task<CursorResult<GameSystem>> BrowseAsync(int pageSize, string? searchPhrase = null, Guid? cursor = null, Guid? ownerId = null);
  void Add(GameSystem gameSystem);
}
