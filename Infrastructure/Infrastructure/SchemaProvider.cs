namespace Infrastructure.Infrastructure;

using System.Text;
using Application.Exceptions;
using Domain.Ports.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

public class SchemaProvider : ISchemaProvider {

  public async Task<Dictionary<string,object>> FetchSchemaAsync(Stream schemaStream) {
    using (StreamReader reader = new(schemaStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024)) {
      string schemaJson = await reader.ReadToEndAsync();
      Dictionary<string, object>? schemaObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(schemaJson);

      if (schemaObject is null) {
        throw new SchemaNotLoadedException("Character sheet schema could not be loaded!");
      }
      
      return schemaObject;
    }
  }
  
  public void ValidateContentWithSchema(Dictionary<string,object> content, Dictionary<string,object> schema) {
    JObject jSchemaObject = JObject.FromObject(schema);
    JSchema jSchema = JSchema.Parse(jSchemaObject.ToString());
    
    JObject jContentObject = JObject.FromObject(content);

    if (jContentObject.IsValid(jSchema, out IList<ValidationError> errors)) {
      return;
    }

    throw new SchemaViolationException(errors.Select(e => $"Property '{e.Path}': {e.Message}").ToList());
  }
}
