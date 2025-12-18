namespace Domain.Ports.Infrastructure;

public interface ISchemaProvider {
  Task<string> FetchSchemaAsync(Stream schemaStream);
  Task<string> GenerateClassHierarchyFromSchemaAsync(string schema);
  void ValidateContentWithSchema(string content, string schema);
}
