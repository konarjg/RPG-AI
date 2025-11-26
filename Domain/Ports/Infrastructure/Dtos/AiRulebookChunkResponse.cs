namespace Domain.Ports.Infrastructure.Dtos;

public record AiRulebookChunkResponse(IEnumerable<RulebookChunk> Chunks);

public record RulebookChunk(RulebookChunkTag Tag, string Content);

public enum RulebookChunkTag {
  Standard,
  CharacterCreation
}