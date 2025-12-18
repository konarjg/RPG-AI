namespace Infrastructure.Infrastructure.Engine;

using System.Reflection;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NJsonSchema;

public class RoslynRuleEngine(ILogger<RoslynRuleEngine> logger) : IRuleEngine {
  private static readonly ScriptOptions ScriptOptions = ScriptOptions.Default
                                                                     .AddReferences(
                                                                       typeof(System.Dynamic.DynamicObject).Assembly,
                                                                       typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly,
                                                                       typeof(System.Linq.Enumerable).Assembly,
                                                                       typeof(System.Collections.Generic.List<>).Assembly,
                                                                       typeof(Newtonsoft.Json.JsonConvert).Assembly,
                                                                       typeof(RoslynRuleContext).Assembly
                                                                     )
                                                                     .AddImports(
                                                                       "System",
                                                                       "System.Linq",
                                                                       "System.Collections.Generic",
                                                                       "Newtonsoft.Json",
                                                                       typeof(RoslynRuleContext).Namespace
                                                                     );


  public async Task<RuleExecutionResult> ExecuteRuleAsync(RuleContext context, CancellationToken cancellationToken = default) {
    RoslynRuleContext ruleContext = new(context);
    string rule = $@"
      {context.SchemaClasses}

      CharacterSheet CharacterState = CharacterSheetRaw.Length != 0 ? JsonConvert.DeserializeObject<CharacterSheet>(CharacterSheetRaw) : new();

      {context.CharacterCreationRule.Replace("\\\"", "\"").Replace("\\n", Environment.NewLine)}

      CharacterSheetRaw = JsonConvert.SerializeObject(CharacterState);
    ";

    try {
      logger.LogInformation($"Executing rule:\n{rule}");
      
      ScriptState<object> scriptState = await CSharpScript.RunAsync(
                                          rule,
                                          ScriptOptions,
                                          globals: ruleContext,
                                          cancellationToken: cancellationToken
                                        );

      if (scriptState.Exception != null) {
        throw new InvalidOperationException("The generated script threw an exception during execution.",scriptState.Exception);
      }

      return new RuleExecutionResult(ruleContext.CharacterSheetRaw,ruleContext.LoggedRolls);
    } catch (CompilationErrorException e) {
      throw new InvalidOperationException($"Rule compilation failed: {string.Join(Environment.NewLine,e.Diagnostics)}.\nRule: {rule}",e);
    } catch (OperationCanceledException) {
      throw new TimeoutException($"The rule execution timed out.\nRule: {rule}");
    }
  }
}
 