namespace Domain.Ports.Infrastructure;

using Dtos;
using Entities;

public interface IRuleEngine {
  void ExecuteRule(Character character, string rule);
}
