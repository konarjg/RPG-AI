namespace Application.Reporters.Interfaces;

using Application.Reporters.Commands;

using Domain.Entities;

public interface ICampaignReporter
{
  Task<Campaign> StartCampaignAsync(StartCampaignCommand command);
}
