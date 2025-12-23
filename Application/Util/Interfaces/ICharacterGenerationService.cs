namespace Application.Util.Interfaces;

using Domain.Entities;

public interface ICharacterGenerationService {
  Task<CharacterGenerationResult> GenerateCharacterAsync(Campaign campaign, GameSystem gameSystem, string? concept = null);
}
