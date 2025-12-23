namespace Application.Providers;

public record BrowseCharactersQuery(
  int PageSize, 
  Guid CampaignId,
  Guid OwnerId,
  Guid? Cursor = null);