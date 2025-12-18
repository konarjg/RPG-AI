namespace Domain.Factories;

using Dtos;
using Entities;
using Interfaces;
using Ports.Infrastructure;
using Ports.Persistence;

public class CampaignFactory(ICampaignRepository campaignRepository, IGuidGenerator guidGenerator, IDateTimeProvider dateTimeProvider) : ICampaignFactory {

  public Campaign CreateCampaign(CreateCampaignCommand command) {
    Campaign campaign = new Campaign() {
      Id = guidGenerator.GenerateGuid(),
      OwnerId = command.OwnerId,
      GameSystemId = command.GameSystemId,
      Title = command.Title,
      Overview = command.Overview,
      StartedAt = dateTimeProvider.GetCurrentDateTime()
    };
    
    campaignRepository.AddCampaign(campaign);
    return campaign;
  }
}
