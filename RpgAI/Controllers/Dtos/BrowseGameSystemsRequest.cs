namespace RpgAI.Controllers.Dtos;

public record BrowseGameSystemsRequest(string SearchPhrase, int PageSize, Guid? Cursor = null);
