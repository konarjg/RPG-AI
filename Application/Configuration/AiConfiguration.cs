namespace Application.Configuration;

public record AiConfiguration {
  public int RecentEventsPageSize { get; init; }
  public int RulesRetrievalLimit { get; init; }
  public int EventsRetrievalLimit { get; init; }
  public int SessionSummariesRetrievalLimit { get; init; }
}
