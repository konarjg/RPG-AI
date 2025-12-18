namespace Domain.Dtos;

using Ports.Infrastructure.Dtos;

public record CreateGameSystemCommand(string Title, string Overview, Guid? OwnerId, string CharacterSheetSchema, string CharacterCreationGuide, List<CreateRulebookChapterCommand> ChapterCommands);

public record CreateRulebookChapterCommand(string Content, string Summary, float[] SummaryEmbedding);