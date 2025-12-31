namespace Application.Providers.Queries;

public record GetCharacterQuery(Guid CampaignId, Guid CharacterId, Guid OwnerId);
