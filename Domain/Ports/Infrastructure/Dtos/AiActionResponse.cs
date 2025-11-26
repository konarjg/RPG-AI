namespace Domain.Ports.Infrastructure.Dtos;

public record AiActionResponse {
  public required string Content { get; init; }
  public string? Action { get; init; }
  
  public int InputTokens { get; init; }
  public int OutputTokens { get; init; }
}
