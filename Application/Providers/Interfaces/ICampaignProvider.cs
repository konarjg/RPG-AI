namespace Application.Providers.Interfaces;

using Domain.Dtos;
using Domain.Entities;

public interface ICampaignProvider {
  Task<Campaign?> GetCampaignDetailsAsync(GetCampaignQuery query);
  Task<CursorResult<Campaign>> BrowseCampaignsAsync(BrowseCampaignsQuery query);
}
