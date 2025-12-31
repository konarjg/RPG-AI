namespace RpgAI.Controllers.Dtos;

public record BrowseSessionsRequest(int PageSize, Guid? Cursor = null);
