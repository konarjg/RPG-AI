namespace Application.Reporters;

using Commands;
using Domain.Dtos;
using Domain.Entities;
using Domain.Factories.Interfaces;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;using Domain.Ports.Persistence;
using Interfaces;

public class MessageReporter(IAiClient aiClient, ISessionRepository sessionRepository, ISceneFactory sceneFactory, IMessageFactory messageFactory) : IMessageReporter {

  public async Task SaveMessageAsync(SaveMessageCommand command) {
    Scene? currentScene = command.Session.CurrentScene;
    
    if (currentScene is null) {
      currentScene = sceneFactory.CreateScene(new CreateSceneCommand(command.Session));
      command.Session.CurrentScene = currentScene;
    }

    Scene scene = currentScene;
    
    if (currentScene.TotalMessages == 20) {
      scene = sceneFactory.CreateScene(new CreateSceneCommand(command.Session));
      command.Session.LastSceneNumber = currentScene.SceneNumber;
      command.Session.CurrentScene = scene;

      AiSummarizeSceneResponse summaryResponse = await aiClient.SummarizeSceneAsync(new AiSummarizeSceneRequest(scene));
      AiEmbedTextResponse embeddingResponse = await aiClient.EmbedTextAsync(new AiEmbedTextRequest(summaryResponse.Summary));

      scene.Summary = summaryResponse.Summary;
      scene.SummaryEmbedding = embeddingResponse.Embedding;
      
      messageFactory.CreateMessage(new CreateMessageCommand(scene,Role.GameMaster,command.Content));
    }
    
    messageFactory.CreateMessage(new CreateMessageCommand(scene,Role.GameMaster,command.Content));
  }
}
