namespace Application;

using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Dtos;
using Exceptions;
using Interfaces;

public class CharacterService(IAiClient aiClient, ISchemaProvider schemaProvider, ICampaignRepository campaignRepository, IUnitOfWork unitOfWork) : ICharacterService{
  
  public async Task<Character> GenerateCharacterAsync(GenerateCharacterCommand command,
    CancellationToken cancellationToken = default) {
    
    Campaign? campaign = await campaignRepository.GetByIdAsync(command.CampaignId, command.UserId, cancellationToken);

    if (campaign is null) {
      throw new NotFoundException<Campaign>(command.CampaignId);
    }

    GameSystem gameSystem = campaign.GameSystem;
    
    AiCreateCharacterRequest createCharacterRequest = new(
      campaign.Overview,
      campaign.GameSystem.CharacterSheetSchema,
      gameSystem.CharacterCreationGuide,
      command.CharacterConcept
    );

    AiCreateCharacterResponse createCharacterResponse = await aiClient.CreateCharacterAsync(createCharacterRequest, cancellationToken);

    schemaProvider.ValidateWithSchema(createCharacterResponse.State,campaign.GameSystem.CharacterSheetSchema);

    Character character = new() {
      Name = createCharacterResponse.Name,
      Overview = createCharacterResponse.Overview,
      CharacterType = CharacterType.Player,
      State = createCharacterResponse.State,
      CampaignId = campaign.Id
    };
      
    campaign.Characters.Add(character);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return character;
  }
}
