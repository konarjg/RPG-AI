namespace Domain.Ports.Infrastructure.Dtos;

public record AiGenerateCharacterRequest(
  string CampaignOverview, 
  string CharacterCreationGuide, 
  string CharacterSheetClassHierarchy,
  string? Concept = null);
