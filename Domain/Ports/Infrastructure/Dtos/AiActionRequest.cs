namespace Domain.Ports.Infrastructure.Dtos;

using Entities;

public record AiActionRequest {
  public required Character Character { get; init; }
  public required string CurrentEvent { get; init; }
  public required IEnumerable<CampaignEvent> RecentEvents { get; init; }
  public required IEnumerable<CampaignEvent> RelevantPastEvents { get; init; }
  public required IEnumerable<Session> RelevantSessions { get; init; }
  public required IEnumerable<GameRuleChunk> RelevantRules { get; init; }
}
