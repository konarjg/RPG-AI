namespace Application.Interfaces;

using Domain.Entities;
using Dtos;

public interface ICharacterService {
  Task<Character> GenerateCharacterAsync(GenerateCharacterCommand command, CancellationToken cancellationToken = default);
}
