namespace Domain.Factories.Interfaces;

using Dtos;
using Entities;

public interface ISceneFactory {
  Scene CreateScene(CreateSceneCommand command);
}
