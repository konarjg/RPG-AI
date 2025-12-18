namespace Domain.Ports.Infrastructure.Dtos;

public record RuleContext {
  public string CharacterSheet { get; init; }
  public string CharacterSheetSchema { get; init; }
  public string CharacterCreationRule { get; init; }
  public string SchemaClasses { get; init; }
  
  public List<int> Roll(int sides, int count) {
    return Enumerable.Range(0, count)
                     .Select(_ => Random.Shared.Next(1, sides + 1))
                     .ToList();
  }
}
