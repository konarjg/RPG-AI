namespace Application.Dtos;

public record GenerateCharacterCommand(Guid CampaignId, Guid UserId, string? CharacterConcept = null);
