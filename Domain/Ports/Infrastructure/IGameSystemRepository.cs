namespace Domain.Ports.Infrastructure;

using Dtos;
using Entities;

public interface IGameSystemRepository {
  Task<GameSystem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<CursorResult<GameSystem>> GetAllAsync(string? cursor, int pageSize, CancellationToken? cancellationToken = default);
    
  void Add(GameSystem gameSystem);
}
