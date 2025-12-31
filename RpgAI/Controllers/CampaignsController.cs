namespace RpgAI.Controllers;

using Application.Reporters.Commands;

using Application.Providers.Queries;
using Application.Providers.Interfaces;
using Application.Reporters;
using Application.Reporters.Interfaces;
using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Exceptions;
using Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RpgAI.Mappers;

[ApiController]
[Route(CampaignRoutes.Base)]
public class CampaignsController(ICampaignReporter campaignReporter, ICampaignProvider campaignProvider) : ControllerBase
{

  [HttpGet(CampaignRoutes.Get)]
  public async Task<ActionResult<CampaignResponse>> GetCampaignDetailsAsync(Guid id)
  {
    Campaign? campaign = await campaignProvider.GetCampaignDetailsAsync(new GetCampaignQuery(id, Guid.AllBitsSet));

    if (campaign is null)
    {
      throw new NotFoundException<Campaign>(id);
    }

    return Ok(CampaignApiMapper.ToResponse(campaign));
  }

  [HttpGet(CampaignRoutes.Browse)]
  public async Task<ActionResult<CursorResult<CampaignResponse>>> BrowseCampaignsAsync([FromQuery] BrowseCampaignsRequest request)
  {
    BrowseCampaignsQuery query = new(
      request.PageSize,
      request.OwnerId,
      request.Cursor,
      request.SearchPhrase);

    CursorResult<Campaign> campaigns = await campaignProvider.BrowseCampaignsAsync(query);

    return Ok(new CursorResult<CampaignResponse>(
      campaigns
        .Items.Select(CampaignApiMapper.ToResponse)
        .ToList(),
      campaigns.NextCursor,
      campaigns.HasMoreItems));
  }

  [HttpPost(CampaignRoutes.Start)]
  public async Task<ActionResult<CampaignResponse>> StartAsync([FromBody] StartCampaignRequest request)
  {
    StartCampaignCommand command = new StartCampaignCommand(
      Guid.AllBitsSet, //Temporary user
      request.Title,
      request.Overview,
      request.GameSystemId,
      request.CharacterConcept);

    Campaign campaign = await campaignReporter.StartCampaignAsync(command);

    return Ok(CampaignApiMapper.ToResponse(campaign));
  }

}
