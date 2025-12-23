namespace Application.Providers;

public record BrowseCampaignsQuery(int PageSize, Guid OwnerId, Guid? Cursor = null, string? SearchPhrase = null);
