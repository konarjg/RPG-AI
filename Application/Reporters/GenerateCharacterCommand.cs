namespace Application.Reporters;

public record GenerateCharacterCommand(Guid CampaignId, Guid OwnerId, string? Concept = null);
