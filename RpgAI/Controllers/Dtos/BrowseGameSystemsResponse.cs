namespace RpgAI.Controllers.Dtos;

public record BrowseGameSystemsResponse(Guid Id, string Title, string Overview, bool IsPublic);
