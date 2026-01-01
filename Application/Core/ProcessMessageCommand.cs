namespace Application.Core;

public record ProcessMessageCommand(Guid CampaignId, Guid OwnerId, string Content);
