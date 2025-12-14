namespace Domain.Dtos;

public record CursorResult<T>(List<T> Items, Guid? NextCursor, bool HasMoreItems);
