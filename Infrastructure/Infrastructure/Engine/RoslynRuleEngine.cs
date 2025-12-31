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
using System.Diagnostics;
using global::Infrastructure.Diagnostics;
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

      CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      cts.CancelAfter(TimeSpan.FromSeconds(10)); // Enforce 10 second timeout (increased for Docker stability)
  
      using Activity? activity = RpgAiActivitySource.Instance.StartActivity("ExecuteRule");
      activity?.SetTag("code.content", rule);
  
      try {
        logger.LogInformation("Executing rule. Length: {RuleLength}", rule.Length);
        
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
  
        ScriptState<object> scriptState = await CSharpScript.RunAsync(
                                            rule,
                                            ScriptOptions,
                                            globals: ruleContext,
                                            cancellationToken: cts.Token
                                          );
      
      stopwatch.Stop();
      logger.LogInformation("Rule executed successfully in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

      if (scriptState.Exception != null) {
        throw new InvalidOperationException("The generated script threw an exception during execution.",scriptState.Exception);
      }

      activity?.SetTag("output.character_sheet", ruleContext.CharacterSheetRaw);
      return new RuleExecutionResult(ruleContext.CharacterSheetRaw,ruleContext.LoggedRolls);
    } catch (CompilationErrorException e) {
      activity?.SetStatus(ActivityStatusCode.Error, "Compilation Failed");
      activity?.SetTag("error.diagnostics", string.Join(Environment.NewLine, e.Diagnostics));
      activity?.AddException(e);
      logger.LogError(e, "Rule compilation failed.");
      throw new InvalidOperationException($"Rule compilation failed: {string.Join(Environment.NewLine,e.Diagnostics)}.\nRule: {rule}",e);
    } catch (OperationCanceledException) {
      activity?.SetStatus(ActivityStatusCode.Error, "Timeout");
      logger.LogWarning("Rule execution timed out.");
      throw new TimeoutException($"The rule execution timed out.\nRule: {rule}");
    } catch (Exception e) {
      activity?.SetStatus(ActivityStatusCode.Error, e.Message);
      activity?.AddException(e);
      logger.LogError(e, "Rule execution failed.");
      throw;
    }
  }
}
 