namespace Domain.Factories;

using Dtos;
using Entities;
using Factories.Interfaces;
using Interfaces;
using Ports.Infrastructure;
using Ports.Infrastructure.Dtos;
using Ports.Persistence;

public class GameSystemFactory(IDateTimeProvider dateTimeProvider, IGuidGenerator guidGenerator) : IGameSystemFactory {
  
  public GameSystem CreateGameSystem(CreateGameSystemCommand command) {
    Guid id = guidGenerator.GeneradeGuid();
    
    GameSystem gameSystem = new() {
      Id = id,
      OwnerId = command.OwnerId,
      Title = command.Title,
      Overview = command.Overview,
      CharacterSheetSchema = command.CharacterSheetSchema,
      UploadedAt = dateTimeProvider.GetCurrentDateTime(),
      Chapters = command.ChapterCommands.Select(chapterCommand => CreateChapter(chapterCommand, id)).ToList()
    };
    
    return gameSystem;
  }

  private RulebookChapter CreateChapter(CreateRulebookChapterCommand command, Guid gameSystemId) {
    return new RulebookChapter() {
      Id = guidGenerator.GeneradeGuid(),
      GameSystemId = gameSystemId,
      Content = command.Content,
      Summary = command.Summary,
      SummaryEmbedding = command.SummaryEmbedding
    };
  }
}
