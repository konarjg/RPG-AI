namespace Application.Reporters.Interfaces;

using Domain.Entities;

public interface ICampaignReporter {
  Task<Campaign> StartCampaignAsync(StartCampaignCommand command);
}
