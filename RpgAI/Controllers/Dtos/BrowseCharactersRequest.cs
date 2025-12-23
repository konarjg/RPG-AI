namespace RpgAI.Controllers.Dtos;

public record BrowseCharactersRequest(int PageSize, Guid? Cursor = null);
