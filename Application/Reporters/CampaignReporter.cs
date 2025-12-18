namespace Application.Reporters;

using Domain.Dtos;
using Domain.Entities;
using Domain.Factories.Interfaces;
using Domain.Ports.Persistence;
using Exceptions;

public class CampaignReporter(ICampaignFactory campaignFactory, IGameSystemRepository gameSystemRepository, ICharacterReporter characterReporter, IUnitOfWork unitOfWork) : ICampaignReporter {

  public async Task<Campaign> StartCampaignAsync(StartCampaignCommand command) {
    GameSystem gameSystem = await gameSystemRepository.GetByIdAsync(command.GameSystemId) ?? throw new NotFoundException<GameSystem>(command.GameSystemId);
    Campaign campaign = campaignFactory.CreateCampaign(new CreateCampaignCommand(command.OwnerId, command.Title, command.Overview, command.GameSystemId));

    await characterReporter.GenerateCharacterAsync(new GenerateCharacterCommand(campaign,gameSystem,command.CharacterConcept));
    //await unitOfWork.SaveChangesAsync();

    return campaign;
  }
}
