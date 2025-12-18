namespace RpgAI.Controllers.Dtos;

public record GameSystemResponse(Guid Id, string Title, string Overview, bool IsPublic, string CharacterCreationGuide, List<RulebookChapterResponse> Chapters);
