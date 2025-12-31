namespace Application.Providers;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Persistence;
using Interfaces;
using Queries;

public class CampaignProvider(ICampaignRepository campaignRepository) : ICampaignProvider
{

  public async Task<Campaign?> GetCampaignDetailsAsync(GetCampaignQuery query)
  {
    return await campaignRepository.GetCampaignDetailsAsync(query.Id, query.OwnerId);
  }

  public async Task<CursorResult<Campaign>> BrowseCampaignsAsync(BrowseCampaignsQuery query)
  {
    return await campaignRepository.BrowseAsync(query.PageSize, query.OwnerId, query.SearchPhrase, query.Cursor);
  }
}
