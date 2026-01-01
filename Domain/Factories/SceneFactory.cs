namespace Domain.Factories;

using Dtos;
using Entities;
using Exceptions;
using Interfaces;
using Ports.Infrastructure;
using Ports.Persistence;

public class SceneFactory(IGuidGenerator guidGenerator, ISceneRepository sceneRepository) : ISceneFactory {

  public Scene CreateScene(CreateSceneCommand command) {
    if (command.Session.CurrentSceneId is not null) {
      throw new AlreadyExistsException($"Session {command.Session.Id} already has an active scene.");
    }

    Scene scene = new() {
      Id = guidGenerator.GenerateGuid(),
      SessionId = command.Session.Id,
      SceneNumber = command.Session.LastSceneNumber ?? 1,
      TotalMessages = 0
    };
    
    sceneRepository.Add(scene);
    
    return scene;
  }
}
