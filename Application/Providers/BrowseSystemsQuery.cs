namespace Application.Providers;

public record BrowseSystemsQuery(string SearchPhrase, int PageSize, Guid? Cursor = null, Guid? OwnerId = null);
