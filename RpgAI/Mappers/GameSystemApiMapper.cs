namespace RpgAI.Mappers;

using Domain.Entities;
using RpgAI.Controllers.Dtos;

public static class GameSystemApiMapper {

  public static GameSystemResponse ToResponse(GameSystem gameSystem) {
    List<RulebookChapterResponse> chapters = gameSystem.Chapters
       .Select(c => new RulebookChapterResponse(c.Content, c.Summary))
       .ToList();

    return new GameSystemResponse(
       gameSystem.Id,
       gameSystem.Title,
       gameSystem.Overview,
       gameSystem.IsPublic,
       gameSystem.CharacterCreationGuide,
       chapters
    );
  }
  
  public static BrowseGameSystemsResponse ToBrowseResponse(GameSystem gameSystem) {
      return new BrowseGameSystemsResponse(gameSystem.Id, gameSystem.Title, gameSystem.Overview, gameSystem.IsPublic);
  }
}
