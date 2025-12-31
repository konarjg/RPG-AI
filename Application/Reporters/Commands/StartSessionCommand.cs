namespace Application.Reporters.Commands;

public record StartSessionCommand(Guid CampaignId, Guid OwnerId);
