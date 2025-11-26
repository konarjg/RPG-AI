namespace Domain.Ports.Infrastructure.Dtos;

public record AiCreateCharacterRequest(string CampaignOverview, Dictionary<string, object> CharacterSheetSchema, string CharacterCreationGuide, string? CharacterConcept = null);
