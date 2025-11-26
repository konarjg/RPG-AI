namespace Application.Dtos;

public record UploadGameSystemCommand(string Title, string Overview, Stream RulebookStream, Stream CharacterSheetSchemaStream);
