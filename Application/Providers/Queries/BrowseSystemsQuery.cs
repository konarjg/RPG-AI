namespace Application.Providers.Queries;

public record BrowseSystemsQuery(int PageSize, string? SearchPhrase = null, Guid? Cursor = null, Guid? OwnerId = null);
