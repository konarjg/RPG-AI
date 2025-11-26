namespace Application.Exceptions;

public class SchemaViolationException(string message, List<string> errors) : Exception(message) {
  public List<string> Errors { get; } = errors;
}
