namespace Domain.Dtos;

using Ports.Infrastructure.Dtos;

public record CreateGameSystemCommand(string Title, string Overview, Guid? OwnerId, Dictionary<string, object> CharacterSheetSchema, List<CreateRulebookChapterCommand> ChapterCommands);

public record CreateRulebookChapterCommand(string Content, string Summary, float[] SummaryEmbedding);