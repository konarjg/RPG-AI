namespace RpgAI.Controllers;

using Application.Reporters;
using Domain.Entities;using Domain.Ports.Infrastructure;
using Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController(ICampaignReporter campaignReporter, ISchemaProvider schemaProvider, ILogger<CampaignsController> logger) : ControllerBase{
  [HttpPost]
  public async Task<ActionResult<Campaign>> StartAsync([FromBody] StartCampaignRequest request) {
    StartCampaignCommand command = new StartCampaignCommand(
      Guid.CreateVersion7(), //Temporary user
      request.Title, 
      request.Overview, 
      request.GameSystemId, 
      request.CharacterConcept);
    
    Campaign campaign = await campaignReporter.StartCampaignAsync(command);
    CampaignResponse response = new(campaign.Id, campaign.Title, campaign.Overview, campaign.GameSystemId, campaign.StartedAt);

    return Ok(response);
  }
}
