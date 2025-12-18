namespace Infrastructure.Infrastructure.AiClient.Util;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.GenAI.Types;
using Type = System.Type; 

public static class GeminiJsonSchemaGenerator {
  
  public static Schema FromType<T>() {
    return Generate(typeof(T));
  }

  private static Schema Generate(Type type) {
    if (Nullable.GetUnderlyingType(type) is { } underlying) {
      return Generate(underlying);
    }
    
    if (type.IsEnum) {
      return new Schema() {
        Type = Google.GenAI.Types.Type.STRING,
        Enum = Enum.GetNames(type).ToList()
      };
    }
    
    return Type.GetTypeCode(type) switch {
      TypeCode.Int16 or TypeCode.Int32 => new Schema() { Type = Google.GenAI.Types.Type.INTEGER, Format = "int32" },
      TypeCode.Int64  => new Schema() { Type = Google.GenAI.Types.Type.INTEGER, Format = "int64" },
      TypeCode.Single => new Schema() { Type = Google.GenAI.Types.Type.NUMBER,  Format = "float" },
      TypeCode.Double or TypeCode.Decimal => new Schema() { Type = Google.GenAI.Types.Type.NUMBER,  Format = "double" },
      TypeCode.Boolean => new Schema() { Type = Google.GenAI.Types.Type.BOOLEAN },
      TypeCode.String => new Schema() { Type = Google.GenAI.Types.Type.STRING },
      TypeCode.DateTime => new Schema() { Type = Google.GenAI.Types.Type.STRING, Format = "date-time" },
      _ => GenerateComplexOrCollection(type)
    };
  }

  private static Schema GenerateComplexOrCollection(Type type) {
    if (type == typeof(Guid)) {
      return new Schema { Type = Google.GenAI.Types.Type.STRING, Format = "uuid" };
    }
    
    if (typeof(IEnumerable).IsAssignableFrom(type)) {
      Type itemType = type.IsArray 
        ? type.GetElementType()! 
        : type.GetGenericArguments().FirstOrDefault() ?? typeof(object);

      return new Schema {
        Type = Google.GenAI.Types.Type.ARRAY,
        Items = Generate(itemType)
      };
    }
    
    Schema schema = new() {
      Type = Google.GenAI.Types.Type.OBJECT,
      Properties = new Dictionary<string, Schema>(),
      Required = new List<string>()
    };

    foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
      schema.Properties[prop.Name] = Generate(prop.PropertyType);
      
      if (Nullable.GetUnderlyingType(prop.PropertyType) == null) {
        schema.Required.Add(prop.Name);
      }
    }

    return schema;
  }
}