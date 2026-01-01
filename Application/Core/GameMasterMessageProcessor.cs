namespace Application.Core;

using Domain.Dtos;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Factories.Interfaces;
using Domain.Ports.Infrastructure;
using Domain.Ports.Persistence;
using Reporters.Commands;
using Reporters.Interfaces;

public class GameMasterMessageProcessor(ICampaignRepository campaignRepository, IAiClient aiClient, IMessageReporter messageReporter) : IGameMasterMessageProcessor {

  public async Task ProcessMessageAsync(ProcessMessageCommand command) {
    Campaign? campaign = await campaignRepository.GetCampaignDetailsAsync(command.CampaignId,command.OwnerId);
    
    if (campaign is null) {
      throw new NotFoundException<Campaign>(command.CampaignId);
    }
    
    Session? session = await campaignRepository.GetCurrentSessionAsync(command.CampaignId, command.OwnerId);

    if (session is null) {
      throw new NotFoundException($"Campaign {command.CampaignId} does not have an active session.");
    }

    await messageReporter.SaveMessageAsync(new SaveMessageCommand(command.OwnerId, session,Role.GameMaster,command.Content));
    AiGenerateReactionResponse reaction = aiClient.GenerateReactionAsync(new AiGenerateReactionResponse(session, campaign));
  }
}
