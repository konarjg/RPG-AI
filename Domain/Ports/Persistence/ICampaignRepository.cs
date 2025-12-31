namespace Domain.Ports.Persistence;

using Dtos;
using Entities;

public interface ICampaignRepository {
  Task<CursorResult<Campaign>> BrowseAsync(
      int pageSize, 
      Guid ownerId,
      string? searchPhrase = null,
      Guid? cursor = null);
  
  Task<Campaign?> GetCampaignDetailsAsync(Guid id, Guid ownerId);
  
  Task<CursorResult<Character>> BrowseCharactersAsync(
      int pageSize, 
      Guid id,
      Guid ownerId,
      Guid? cursor = null);
  
  Task<Character?> GetCharacterByIdAsync(Guid id, Guid characterId, Guid ownerId);

  Task<Session?> GetCurrentSessionAsync(Guid id, Guid ownerId);
  
  void AddCampaign(Campaign campaign);
  void AddCharacter(Character character);
}