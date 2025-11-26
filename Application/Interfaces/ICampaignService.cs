namespace Application.Interfaces;

using Domain.Entities;
using Domain.Ports.Infrastructure.Dtos;
using Dtos;

public interface ICampaignService {
  Task<Campaign?> GetCampaignStateAsync(Guid id,
    Guid userId,
    CancellationToken cancellationToken = default);

  Task<List<Campaign>> GetAllCampaignsAsync(Guid userId, CancellationToken cancellationToken = default);

  Task<Campaign> CreateCampaignAsync(CreateCampaignCommand command, CancellationToken cancellationToken = default);
}
