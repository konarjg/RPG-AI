namespace Domain.Entities;

public class CampaignEvent {
  public Guid Id { get; init; } = Guid.CreateVersion7();
  public required Guid CampaignId { get; set; }
  public required Guid SessionId { get; set; }
  public required string Content { get; set; }
  public required float[] ContentEmbedding { get; set; }
  public Guid? CharacterId { get; set; }
  public required DateTime OccuredAt { get; set; } 
  
  public virtual Campaign? Campaign { get; set; }
  public virtual Session? Session { get; set; }
  public virtual Character? Character { get; set; }
}
