namespace Application.Reporters;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Domain.Ports.Persistence;
using Domain.Factories.Interfaces;

public class GameSystemReporter(IGameSystemRepository gameSystemRepository, IGameSystemFactory gameSystemFactory, IAiClient aiClient, ISchemaProvider schemaProvider, IUnitOfWork unitOfWork) : IGameSystemReporter {

  public async Task<GameSystem> UploadGameSystemAsync(UploadGameSystemCommand command) {
    AiSplitRulebookResponse splitRulebookResponse = await aiClient.SplitRulebookAsync(new AiSplitRulebookRequest(command.RulebookStream, command.RulebookContentType));
    List<AiEmbedTextResponse> embeddings = await aiClient.EmbedAllTextsAsync(splitRulebookResponse.Entries
                                                                                                  .Select(entry => entry.Summary)
                                                                                                  .Select(summary => new AiEmbedTextRequest(summary))
                                                                                                  .ToList());
    if (splitRulebookResponse.Entries.Count != embeddings.Count) {
      throw new InvalidOperationException(
        $"Embedding mismatch! Sent {splitRulebookResponse.Entries.Count} summaries but received {embeddings.Count} embeddings.");
    }
    
    
    List<CreateRulebookChapterCommand> rulebookChapterCommands = splitRulebookResponse.Entries
                                                                                      .Zip(embeddings)
                                                                                      .Select(g =>
                                                                                        new CreateRulebookChapterCommand(g.First.Content,
                                                                                          g.First.Summary,g.Second.Embedding))
                                                                                      .ToList();

    Dictionary<string,object> characterSheetSchema = await schemaProvider.FetchSchemaAsync(command.CharacterSheetSchemaStream);

    CreateGameSystemCommand createGameSystemCommand = new(command.Title,command.Overview,command.UserId,characterSheetSchema,rulebookChapterCommands);
    
    GameSystem gameSystem = gameSystemFactory.CreateGameSystem(createGameSystemCommand);

    gameSystemRepository.Add(gameSystem);
    await unitOfWork.SaveChangesAsync();
    
    return gameSystem;
  }
}
