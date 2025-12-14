namespace Domain.Ports.Infrastructure;

public interface ISchemaProvider {
  Task<Dictionary<string,object>> FetchSchemaAsync(Stream schemaStream);
  void ValidateContentWithSchema(Dictionary<string,object> content, Dictionary<string, object> schema);
}
