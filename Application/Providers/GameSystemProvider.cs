namespace Application.Providers;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Persistence;
using Exceptions;

public class GameSystemProvider(IGameSystemRepository gameSystemRepository) : IGameSystemProvider {

  public async Task<GameSystem?> GetGameSystemByIdAsync(GetSystemQuery query) {
    return await gameSystemRepository.GetByIdAsync(query.Id,query.OwnerId);
  }

  public async Task<CursorResult<GameSystem>> BrowseGameSystemsAsync(BrowseSystemsQuery query) {
    return await gameSystemRepository.BrowseAsync(query.PageSize, query.SearchPhrase, query.Cursor, query.OwnerId);
  }
}
