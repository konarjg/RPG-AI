namespace Application.Providers;

public record BrowseSystemsQuery(int PageSize, string? SearchPhrase = null, Guid? Cursor = null, Guid? OwnerId = null);
