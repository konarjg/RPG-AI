namespace RpgAI.Controllers;

using Application.Providers;
using Application.Providers.Interfaces;
using Application.Reporters;
using Application.Reporters.Interfaces;
using Domain.Dtos;
using Domain.Entities;using Domain.Ports.Infrastructure;
using Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[ApiController]
[Route(CampaignRoutes.Base)]
public class CampaignsController(ICampaignReporter campaignReporter,ICampaignProvider campaignProvider) : ControllerBase {

  [HttpGet(CampaignRoutes.Get)]
  public async Task<ActionResult<CampaignResponse>> GetCampaignDetailsAsync(Guid id) {
    Campaign? campaign = await campaignProvider.GetCampaignDetailsAsync(new GetCampaignQuery(id,Guid.AllBitsSet));
    
    return campaign is not null ? Ok(MapResponse(campaign)) : NotFound();
  }

  [HttpGet(CampaignRoutes.Browse)]
  public async Task<ActionResult<CursorResult<CampaignResponse>>> BrowseCampaignsAsync([FromQuery] BrowseCampaignsRequest request) {
    BrowseCampaignsQuery query = new(
      request.PageSize,
      request.OwnerId,
      request.Cursor,
      request.SearchPhrase);

    CursorResult<Campaign> campaigns = await campaignProvider.BrowseCampaignsAsync(query);

    return Ok(new CursorResult<CampaignResponse>(
      campaigns
        .Items.Select(MapResponse)
        .ToList(),
      campaigns.NextCursor,
      campaigns.HasMoreItems));
  }

  [HttpPost(CampaignRoutes.Start)]
  public async Task<ActionResult<CampaignResponse>> StartAsync([FromBody] StartCampaignRequest request) {
    StartCampaignCommand command = new StartCampaignCommand(
      Guid.AllBitsSet, //Temporary user
      request.Title,
      request.Overview,
      request.GameSystemId,
      request.CharacterConcept);

    Campaign campaign = await campaignReporter.StartCampaignAsync(command);

    return Ok(MapResponse(campaign));
  }

  private CampaignResponse MapResponse(Campaign campaign) {
    return new CampaignResponse(campaign.Id,campaign.Title,campaign.Overview,campaign.GameSystemId,campaign.StartedAt);
  }

}
