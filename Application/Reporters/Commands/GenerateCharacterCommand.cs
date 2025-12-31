namespace Application.Reporters.Commands;

public record GenerateCharacterCommand(Guid CampaignId, Guid OwnerId, string? Concept = null);
