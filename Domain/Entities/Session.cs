namespace Domain.Entities;

public class Session {
  public Guid Id { get; init; } = Guid.CreateVersion7();
  public required Guid CampaignId { get; set; }
  public required string Title { get; set; }
  public SessionStatus Status { get; set; } = SessionStatus.Active;
  public string? Summary { get; set; }
  public float[]? SummaryEmbedding { get; set; }
  public required int SessionNumber { get; set; }
  
  public virtual Campaign? Campaign { get; set; }
}

public enum SessionStatus {
  Active,
  Completed
}
