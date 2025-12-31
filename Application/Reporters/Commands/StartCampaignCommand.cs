namespace Application.Reporters.Commands;

public record StartCampaignCommand(Guid OwnerId, string Title, string Overview, Guid GameSystemId, string? CharacterConcept = null);
