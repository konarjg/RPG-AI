namespace Domain.Ports.Infrastructure.Dtos;

public record AiCreateCharacterResponse(string Name, string Overview, Dictionary<string, object> State);
