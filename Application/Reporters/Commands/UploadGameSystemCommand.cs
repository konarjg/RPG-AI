namespace Application.Reporters.Commands;

public record UploadGameSystemCommand(string Title, string Overview, Stream RulebookStream, string RulebookContentType, Stream CharacterSheetSchemaStream, Guid? UserId = null);