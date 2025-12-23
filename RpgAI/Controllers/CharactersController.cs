namespace RpgAI.Controllers;

using Application.Providers;
using Application.Providers.Interfaces;
using Application.Reporters;
using Application.Reporters.Interfaces;
using Domain.Dtos;
using Domain.Entities;
using Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

[ApiController]
[Route(CharacterRoutes.Base)]
public class CharactersController(ICharacterProvider characterProvider, ICharacterReporter characterReporter) : ControllerBase {
  [HttpGet(CharacterRoutes.Get)]
  [ActionName(nameof(GetAsync))]
  public async Task<ActionResult<CharacterResponse>> GetAsync(Guid campaignId, Guid id) {
    Character? character = await characterProvider.GetCharacterByIdAsync(new GetCharacterQuery(campaignId,id,Guid.AllBitsSet));

    if (character is null) {
      return NotFound();
    }

    return Ok(MapResponse(character));
  }

  [HttpGet(CharacterRoutes.Browse)]
  public async Task<ActionResult<CursorResult<BrowseCharactersResponse>>> BrowseAsync(Guid campaignId, [FromQuery] BrowseCharactersRequest request) {
    BrowseCharactersQuery query = new(request.PageSize,campaignId,Guid.AllBitsSet,request.Cursor);
    CursorResult<Character> characters = await characterProvider.BrowseCharacterDetailsAsync(query);
    CursorResult<BrowseCharactersResponse> response = new(characters.Items.Select(c => new BrowseCharactersResponse(
                                                                      c.Id,
                                                                      c.Name,
                                                                      c.Overview,
                                                                      c.CampaignId))
                                                                    .ToList(),characters.NextCursor,characters.HasMoreItems);

    return Ok(response);
  }
  
  [HttpPost(CharacterRoutes.Generate)]
  public async Task<ActionResult<CharacterResponse>> GenerateAsync(Guid campaignId, [FromBody] GenerateCharacterRequest request) {
    GenerateCharacterCommand command = new(campaignId,Guid.AllBitsSet,request.Concept);
    Character character = await characterReporter.GenerateCharacterAsync(command);

    return CreatedAtAction(
      nameof(GetAsync), 
      new { campaignId = command.CampaignId, id = character.Id }, 
      MapResponse(character));
  }

  private CharacterResponse MapResponse(Character character) {
    return new CharacterResponse(character.Id,character.Name,character.Overview,character.CampaignId,JObject.Parse(character.State));
  }
}
