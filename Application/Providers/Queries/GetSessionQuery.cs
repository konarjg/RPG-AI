namespace Application.Providers.Queries;

public record GetSessionQuery(Guid CampaignId, Guid OwnerId, Guid Id);
