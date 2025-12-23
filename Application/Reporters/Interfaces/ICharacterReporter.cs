namespace Application.Reporters.Interfaces;

using Domain.Entities;

public interface ICharacterReporter {
  Task<Character> GenerateCharacterAsync(AutoGenerateCharacterCommand command);
  Task<Character> GenerateCharacterAsync(GenerateCharacterCommand command);
}
