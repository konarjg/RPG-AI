namespace Application.Providers;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Persistence;
using Interfaces;

public class CharacterProvider(ICampaignRepository campaignRepository) : ICharacterProvider {

  public async Task<Character?> GetCharacterByIdAsync(GetCharacterQuery query) {
    return await campaignRepository.GetCharacterByIdAsync(query.CampaignId, query.CharacterId, query.OwnerId);
  }

  public async Task<CursorResult<Character>> BrowseCharacterDetailsAsync(BrowseCharactersQuery query) {
    return await campaignRepository.BrowseCharactersAsync(query.PageSize,query.CampaignId,query.OwnerId,query.Cursor);
  }
}
