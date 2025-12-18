namespace RpgAI.Controllers.Dtos;

public record CampaignResponse(Guid Id, string Title, string Overview, Guid GameSystemId, DateTime StartedAt);
