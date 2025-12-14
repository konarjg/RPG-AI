namespace Application.Providers;

using Domain.Dtos;
using Domain.Entities;

public interface IGameSystemProvider {
  Task<GameSystem?> GetGameSystemByIdAsync(GetSystemQuery query);
  Task<CursorResult<GameSystem>> BrowseGameSystemsAsync(BrowseSystemsQuery query);
}
