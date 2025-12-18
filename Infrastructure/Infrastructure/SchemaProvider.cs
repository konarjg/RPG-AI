namespace Infrastructure.Infrastructure;

using System.Text;
using System.Text.RegularExpressions;
using Application.Exceptions;
using Domain.Ports.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NJsonSchema.CodeGeneration.CSharp;
using JsonSchema = NJsonSchema.JsonSchema;

public class SchemaProvider : ISchemaProvider {

  public async Task<string> FetchSchemaAsync(Stream schemaStream) {
    using StreamReader reader = new(schemaStream,Encoding.UTF8,detectEncodingFromByteOrderMarks: true,bufferSize: 1024);
    return await reader.ReadToEndAsync();
  }
  public async Task<string> GenerateClassHierarchyFromSchemaAsync(string schema) {
    JsonSchema jsonSchema = await JsonSchema.FromJsonAsync(schema);
    
    CSharpGenerator generator = new(jsonSchema,
      new CSharpGeneratorSettings() {
        ClassStyle = CSharpClassStyle.Poco,
        GenerateJsonMethods = false,
        GenerateDefaultValues = true
      });
    
    string raw = generator.GenerateFile();

    Regex namespaceContentRegex = new(@"namespace.*?\{(.*)\}", RegexOptions.Singleline);
    Match match = namespaceContentRegex.Match(raw);
    string contentToProcess = match.Success ? match.Groups[1].Value.Trim() : raw;
    
    string[] lines = contentToProcess.Split([ '\r', '\n' ], StringSplitOptions.RemoveEmptyEntries);
    
    IEnumerable<string> cleanedLines = lines
                                       .Where(line => !line.Trim().StartsWith("["))
                                       .Where(line => !line.Trim().StartsWith("//--") && !line.Trim().StartsWith("#pragma"));
    
    return string.Join(Environment.NewLine, cleanedLines);
  }
 
  public void ValidateContentWithSchema(string content, string schema) {
    JSchema jSchema = JSchema.Parse(content);
    JObject jContentObject = JObject.Parse(content);

    if (jContentObject.IsValid(jSchema, out IList<ValidationError> errors)) {
      return;
    }

    throw new SchemaViolationException(errors.Select(e => $"Property '{e.Path}': {e.Message}").ToList());
  }
}
