namespace Domain.Ports.Infrastructure;

using Dtos;

public interface IRuleEngine {
  Task<RuleExecutionResult> ExecuteRuleAsync(RuleContext context, CancellationToken cancellationToken = default);
}
