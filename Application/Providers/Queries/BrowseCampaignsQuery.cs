namespace Application.Providers.Queries;

public record BrowseCampaignsQuery(int PageSize, Guid OwnerId, Guid? Cursor = null, string? SearchPhrase = null);
