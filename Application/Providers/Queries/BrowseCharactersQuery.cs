namespace Application.Providers.Queries;

public record BrowseCharactersQuery(
  int PageSize,
  Guid CampaignId,
  Guid OwnerId,
  Guid? Cursor = null);