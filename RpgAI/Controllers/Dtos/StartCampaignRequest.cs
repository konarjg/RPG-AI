namespace RpgAI.Controllers.Dtos;

public record StartCampaignRequest(string Title, string Overview, Guid GameSystemId, string? CharacterConcept = null);
