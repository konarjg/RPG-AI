namespace Domain.Factories;

using Dtos;
using Entities;
using Interfaces;
using Ports.Infrastructure;
using Ports.Persistence;

public class CharacterFactory(ICampaignRepository campaignRepository, IGuidGenerator guidGenerator) : ICharacterFactory {

  public Character CreateCharacter(CreateCharacterCommand command) {
    Character character = new() {
      Id = guidGenerator.GenerateGuid(),
      CampaignId = command.CampaignId,
      Name = command.Name,
      Overview = command.Overview,
      State = command.InitialState
    };
    
    campaignRepository.AddCharacter(character);
    return character;
  }
}
