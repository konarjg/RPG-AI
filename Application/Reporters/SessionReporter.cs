namespace Application.Reporters;

using Application.Reporters.Commands;

using Domain.Dtos;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Factories.Interfaces;
using Domain.Ports.Persistence;
using Interfaces;

public class SessionReporter(ICampaignRepository campaignRepository, ISessionFactory sessionFactory, IUnitOfWork unitOfWork) : ISessionReporter
{

  public async Task<Session> StartSessionAsync(StartSessionCommand command)
  {
    Campaign? campaign = await campaignRepository.GetCampaignDetailsAsync(command.CampaignId, command.OwnerId);

    if (campaign is null)
    {
      throw new NotFoundException<Campaign>(command.CampaignId);
    }

    Session session = sessionFactory.CreateSession(new CreateSessionCommand(campaign));
    campaign.CurrentSessionId = session.Id;

    await unitOfWork.SaveChangesAsync();
    return session;
  }

  public async Task EndSessionAsync(EndSessionCommand command)
  {
    Campaign? campaign = await campaignRepository.GetCampaignDetailsAsync(command.CampaignId, command.OwnerId);

    if (campaign is null)
    {
      throw new NotFoundException<Campaign>(command.CampaignId);
    }

    if (campaign.CurrentSessionId is null)
    {
      throw new NotFoundException($"Campaign with id {command.CampaignId} does not have an active current session.");
    }

    Session? session = await campaignRepository.GetCurrentSessionAsync(campaign.Id, campaign.OwnerId);

    if (session is null)
    {
      throw new NotFoundException($"Current session for campaign with id {command.CampaignId} does not exist.");
    }

    campaign.LastSessionNumber = session.SessionNumber;
    campaign.CurrentSessionId = null;

    await unitOfWork.SaveChangesAsync();
  }
}
