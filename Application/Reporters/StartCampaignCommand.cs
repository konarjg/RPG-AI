namespace Application.Reporters;

public record StartCampaignCommand(Guid OwnerId, string Title,  string Overview, Guid GameSystemId, string? CharacterConcept = null);
