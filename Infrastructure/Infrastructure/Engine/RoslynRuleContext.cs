namespace Infrastructure.Infrastructure.Engine;

using Domain.Ports.Infrastructure.Dtos;
using Newtonsoft.Json;
using NJsonSchema;

public class RoslynRuleContext(RuleContext context) {
  public string CharacterSheetRaw { get; set; } = context.CharacterSheet;
  
  public List<RollResult> LoggedRolls { get; private set; } = new();

  public List<int> Roll(int sides, int count) {
    List<int> results = context.Roll(count, sides);
    LoggedRolls.AddRange(new RollResult(sides,results));
    return results;
  }
}
