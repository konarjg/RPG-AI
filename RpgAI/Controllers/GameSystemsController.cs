namespace RpgAI.Controllers;

using Application.Exceptions;
using Application.Providers;
using Application.Reporters;
using Domain.Dtos;
using Domain.Entities;
using Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GameSystemsController(IGameSystemProvider provider, IGameSystemReporter reporter) : ControllerBase {
  [HttpGet("{id}")]
  public async Task<ActionResult<GameSystemResponse>> GetAsync(Guid id) {
    GetSystemQuery query = new(id,null);
    GameSystem? gameSystem = await provider.GetGameSystemByIdAsync(query);

    if (gameSystem is null) {
      return NotFound();
    }
    
    return Ok(MapResponse(gameSystem));
  }

  [HttpPost("/browse")]
  public async Task<ActionResult<CursorResult<GameSystemResponse>>> BrowseAsync([FromBody] BrowseGameSystemsRequest request) {
    BrowseSystemsQuery query = new(request.SearchPhrase, request.PageSize, request.Cursor, null);
    CursorResult<GameSystem> gameSystems = await provider.BrowseGameSystemsAsync(query);
    CursorResult<GameSystemResponse> response = new(gameSystems.Items.Select(MapResponse).ToList(),gameSystems.NextCursor,gameSystems.HasMoreItems);
    
    return Ok(response);
  }

  [HttpPost]
  [Consumes("multipart/form-data")] 
  public async Task<ActionResult<GameSystemResponse>> UploadAsync([FromForm] UploadGameSystemRequest request,
    IFormFile rulebookFile,
    IFormFile schemaFile) {
    
    await using Stream rulebookStream = rulebookFile.OpenReadStream();
    await using Stream schemaStream = schemaFile.OpenReadStream();

    UploadGameSystemCommand command = new(request.Title,request.Overview,rulebookStream, rulebookFile.ContentType,schemaStream);
    GameSystem gameSystem = await reporter.UploadGameSystemAsync(command);

    return Ok(
      MapResponse(gameSystem));
  }

  private GameSystemResponse MapResponse(GameSystem gameSystem) {
    List<RulebookChapterResponse> chapters = gameSystem.Chapters
                                                       .Select(c => 
                                                         new RulebookChapterResponse(
                                                           c.Content,
                                                           c.Summary
                                                         )
                                                       )
                                                       .ToList();

    GameSystemResponse response = new(gameSystem.Id,
      gameSystem.Title,
      gameSystem.Overview,
      gameSystem.IsPublic,
      chapters
    );

    return response;
  }
}
