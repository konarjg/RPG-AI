namespace Domain.Factories.Interfaces;

using Dtos;
using Entities;

public interface ICharacterFactory {
  Character CreateCharacter(CreateCharacterCommand command);
}
