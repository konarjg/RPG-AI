namespace Application.Reporters;

using Application.Reporters.Commands;

using Domain.Dtos;
using Domain.Entities;
using Domain.Factories.Interfaces;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Domain.Ports.Persistence;
using Domain.Exceptions;
using Interfaces;
using Util;
using Util.Interfaces;

public class CharacterReporter(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork, ICharacterFactory characterFactory, ICharacterGenerationService characterGenerationService) : ICharacterReporter
{

  public async Task<Character> GenerateCharacterAsync(AutoGenerateCharacterCommand command)
  {
    return await GenerateAsync(command.Campaign, command.GameSystem, command.Concept);
  }

  public async Task<Character> GenerateCharacterAsync(GenerateCharacterCommand command)
  {
    Campaign campaign = await campaignRepository.GetCampaignDetailsAsync(command.CampaignId, command.OwnerId)
                        ?? throw new NotFoundException<Campaign>(command.CampaignId);

    Character character = await GenerateAsync(campaign, campaign.GameSystem, command.Concept);
    await unitOfWork.SaveChangesAsync();

    return character;
  }

  private async Task<Character> GenerateAsync(Campaign campaign, GameSystem gameSystem, string? concept = null)
  {
    CharacterGenerationResult result = await characterGenerationService.GenerateCharacterAsync(campaign, gameSystem, concept);

    return characterFactory.CreateCharacter(new CreateCharacterCommand(
      campaign.Id,
      result.GenerateCharacterResponse.Name,
      result.GenerateCharacterResponse.Overview,
      result.RuleExecutionResult.FinalCharacterSheet));
  }
}
