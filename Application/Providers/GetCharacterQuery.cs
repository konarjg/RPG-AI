namespace Application.Providers;

public record GetCharacterQuery(Guid CampaignId, Guid CharacterId, Guid OwnerId);
