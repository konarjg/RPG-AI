namespace Application.Reporters.Commands;

public record EndSessionCommand(Guid CampaignId, Guid OwnerId);
