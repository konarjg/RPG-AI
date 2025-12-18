namespace Application.Reporters;

using Domain.Entities;

public interface ICharacterReporter {
  Task<Character> GenerateCharacterAsync(GenerateCharacterCommand command);
}
