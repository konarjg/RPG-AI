namespace Application.Providers;

using Application.Providers.Queries;
using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Persistence;
using Interfaces;

public class SessionProvider(ISessionRepository sessionRepository, ICampaignRepository campaignRepository) : ISessionProvider
{

  public async Task<Session?> GetCurrentSessionAsync(GetCurrentSessionQuery query)
  {
    return await campaignRepository.GetCurrentSessionAsync(query.CampaignId, query.OwnerId);
  }

  public async Task<Session?> GetSessionByIdAsync(GetSessionQuery query)
  {
    return await sessionRepository.GetByIdAsync(query.CampaignId, query.Id, query.OwnerId);
  }

  public async Task<CursorResult<Session>> BrowseSessionsAsync(BrowseSessionsQuery query)
  {
    return await sessionRepository.BrowseAsync(query.CampaignId, query.OwnerId, query.PageSize, query.Cursor);
  }
}
