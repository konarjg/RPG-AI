namespace Application;

using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Dtos;
using Exceptions;
using Interfaces;

public class CampaignService(ICampaignRepository campaignRepository, IUserRepository userRepository, IGameSystemRepository gameSystemRepository, IUnitOfWork unitOfWork) : ICampaignService {
  
  public async Task<Campaign?> GetCampaignStateAsync(Guid id,
    Guid userId,
    CancellationToken cancellationToken = default) {

    return await campaignRepository.GetByIdAsync(id,userId,cancellationToken);
  }

  public async Task<List<Campaign>> GetAllCampaignsAsync(Guid userId,
    CancellationToken cancellationToken = default) {

    return await campaignRepository.GetAllAsync(userId,cancellationToken);
  }
  
  public async Task<Campaign> CreateCampaignAsync(CreateCampaignCommand command,
    CancellationToken cancellationToken = default) {

    if (await userRepository.GetByIdAsync(command.GameMasterUserId, cancellationToken) is null) {
      throw new NotFoundException<User>(command.GameMasterUserId);
    }

    if (await gameSystemRepository.GetByIdAsync(command.GameSystemId,cancellationToken) is null) {
      throw new NotFoundException<GameSystem>(command.GameSystemId);
    }

    Campaign campaign = new Campaign() {
      Name = command.Name,
      Overview = command.Overview,
      GameMasterUserId = command.GameMasterUserId,
      GameSystemId = command.GameSystemId
    };
    
    campaignRepository.Add(campaign);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return campaign;
  }
}
