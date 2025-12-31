namespace Application.Providers.Interfaces;

using Domain.Dtos;
using Domain.Entities;
using Queries;

public interface ICharacterProvider
{
  Task<Character?> GetCharacterByIdAsync(GetCharacterQuery query);
  Task<CursorResult<Character>> BrowseCharacterDetailsAsync(BrowseCharactersQuery query);
}
