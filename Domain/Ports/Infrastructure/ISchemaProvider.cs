namespace Domain.Ports.Infrastructure;

public interface ISchemaProvider {
  Task<Dictionary<string,object>> LoadSchemaAsync(Stream schemaStream,
    CancellationToken cancellationToken = default);

  void ValidateWithSchema(Dictionary<string,object> data,
    Dictionary<string,object> schema);
}
