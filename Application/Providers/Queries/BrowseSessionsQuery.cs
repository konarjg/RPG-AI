namespace Application.Providers.Queries;

public record BrowseSessionsQuery(Guid CampaignId, Guid OwnerId, int PageSize, Guid? Cursor = null);
