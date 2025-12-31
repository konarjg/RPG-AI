namespace RpgAI.Mappers;

using Domain.Entities;
using RpgAI.Controllers.Dtos;

public static class CampaignApiMapper
{

    public static CampaignResponse ToResponse(Campaign campaign)
    {
        return new CampaignResponse(campaign.Id, campaign.Title, campaign.Overview, campaign.GameSystemId, campaign.StartedAt);
    }
}
