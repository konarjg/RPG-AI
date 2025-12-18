namespace Domain.Ports.Infrastructure.Dtos;

public record RuleExecutionResult(string FinalCharacterSheet, List<RollResult> Rolls);

public record RollResult(int Sides, List<int> Results);
