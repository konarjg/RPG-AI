namespace Application.Interfaces;

using Domain.Entities;
using Domain.Ports.Infrastructure.Dtos;
using Dtos;

public interface IGameSystemService {
  Task<GameSystem?> GetSystemByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<CursorResult<GameSystem>> BrowseSystemsAsync(string? cursor, int pageSize, CancellationToken cancellationToken = default);
  Task<GameSystem> UploadSystemAsync(UploadGameSystemCommand command, CancellationToken cancellationToken = default);
}
