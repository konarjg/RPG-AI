namespace Domain.Ports.Infrastructure;

using Entities;

public interface IEventBroadcaster {
  Task BroadcastEventAsync(CampaignEvent campaignEvent, CancellationToken cancellationToken = default);
}
