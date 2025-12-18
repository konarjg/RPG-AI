namespace Application.Reporters;

using Domain.Entities;

public interface ICampaignReporter {
  Task<Campaign> StartCampaignAsync(StartCampaignCommand command);
}
