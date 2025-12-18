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
      Guid campaignId,
      Guid ownerId,
      Guid? cursor = null);
  
  Task<Character?> GetCharacterByIdAsync(Guid campaignId, Guid characterId, Guid ownerId);
  
  Task<CursorResult<Session>> BrowseSessionsAsync(
      int pageSize, 
      Guid campaignId, 
      Guid ownerId,
      DateTime? cursor = null);
  
  void AddCampaign(Campaign campaign);
  void AddSession(Session session);
  void AddCharacter(Character character);
}