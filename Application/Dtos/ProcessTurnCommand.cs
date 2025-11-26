namespace Application.Dtos;

public record ProcessTurnCommand(Guid CampaignId, Guid UserId, Guid NextCharacterId, string GameMasterMessage, DateTime SentAt);
