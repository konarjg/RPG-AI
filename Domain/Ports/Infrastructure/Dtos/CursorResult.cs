namespace Domain.Ports.Infrastructure.Dtos;

public record CursorResult<T> {
  public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
  public string? NextCursor { get; init; } 
  public bool HasMore { get; init; }
}
