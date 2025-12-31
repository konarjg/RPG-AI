namespace RpgAI.Controllers.Dtos;

public record SessionResponse(Guid Id, Guid CampaignId, int SessionNumber, string? Summary, Guid? CurrentSceneId);
