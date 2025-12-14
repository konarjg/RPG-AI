namespace Application.Reporters;

public record UploadGameSystemCommand(string Title, string Overview, Stream RulebookStream, string RulebookContentType, Stream CharacterSheetSchemaStream, Guid? UserId = null);