namespace Domain.Factories.Interfaces;

using Dtos;
using Entities;

public interface ICampaignFactory {
  Campaign CreateCampaign(CreateCampaignCommand command);
}
