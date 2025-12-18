namespace Domain.Ports.Infrastructure.Dtos;

public record AiGenerateCharacterResponse(string Name,string Overview,string CharacterCreationRule);