namespace Application.Exceptions;

public class SchemaViolationException(IList<string> errors) : Exception($"Character sheet schema violated. Errors: {string.Join(Environment.NewLine, errors)}")
{

}
