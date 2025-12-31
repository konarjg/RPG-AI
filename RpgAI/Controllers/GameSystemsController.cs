namespace RpgAI.Controllers;

using Application.Exceptions;
using Application.Providers;
using Application.Providers.Interfaces;
using Application.Reporters;
using Application.Reporters.Interfaces;
using Domain.Dtos;
using Domain.Entities;
using Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route(GameSystemRoutes.Base)]
public class GameSystemsController(IGameSystemProvider provider, IGameSystemReporter reporter) : ControllerBase {
  [HttpGet(GameSystemRoutes.Get)]
  [ActionName(nameof(GetAsync))]
  public async Task<ActionResult<GameSystemResponse>> GetAsync(Guid id) {
    GetSystemQuery query = new(id,null);
    GameSystem? gameSystem = await provider.GetGameSystemByIdAsync(query);

    if (gameSystem is null) {
      return NotFound();
    }
    
    return Ok(RpgAI.Mappers.GameSystemApiMapper.ToResponse(gameSystem));
  }

  [HttpGet(GameSystemRoutes.Browse)]
  public async Task<ActionResult<CursorResult<BrowseGameSystemsResponse>>> BrowseAsync([FromQuery] BrowseGameSystemsRequest request) {
    BrowseSystemsQuery query = new(request.PageSize, request.SearchPhrase, request.Cursor, null);
    CursorResult<GameSystem> gameSystems = await provider.BrowseGameSystemsAsync(query);
    CursorResult<BrowseGameSystemsResponse> response = new(gameSystems.Items.Select(RpgAI.Mappers.GameSystemApiMapper.ToBrowseResponse)
                                                                      .ToList(),gameSystems.NextCursor,gameSystems.HasMoreItems);
    
    return Ok(response);
  }

  [HttpPost(GameSystemRoutes.Upload)]
  [Consumes("multipart/form-data")] 
  public async Task<ActionResult<GameSystemResponse>> UploadAsync([FromForm] UploadGameSystemRequest request,
    IFormFile rulebookFile,
    IFormFile schemaFile) {
    
    await using Stream rulebookStream = rulebookFile.OpenReadStream();
    await using Stream schemaStream = schemaFile.OpenReadStream();

    UploadGameSystemCommand command = new(request.Title,request.Overview,rulebookStream, rulebookFile.ContentType,schemaStream);
    GameSystem gameSystem = await reporter.UploadGameSystemAsync(command);

    return CreatedAtAction(nameof(GetAsync), new { id = gameSystem.Id }, RpgAI.Mappers.GameSystemApiMapper.ToResponse(gameSystem));
  }
}
