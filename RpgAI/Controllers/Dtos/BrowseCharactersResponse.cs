namespace RpgAI.Controllers.Dtos;

public record BrowseCharactersResponse(Guid Id, string Name, string Overview, Guid CampaignId);
