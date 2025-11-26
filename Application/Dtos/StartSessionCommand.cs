namespace Application.Dtos;

public record StartSessionCommand(Guid CampaignId, Guid UserId, string Title);
