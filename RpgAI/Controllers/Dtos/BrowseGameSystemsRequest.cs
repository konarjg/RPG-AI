namespace RpgAI.Controllers.Dtos;

public record BrowseGameSystemsRequest(int PageSize, string? SearchPhrase = null, Guid? Cursor = null);
