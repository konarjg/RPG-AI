namespace Domain.Entities;

public class Campaign {
  public Guid Id { get; init; } = Guid.CreateVersion7();
  public required Guid GameMasterUserId { get; set; }
  public required Guid GameSystemId { get; set; }
  public required string Name { get; set; }
  public required string Overview { get; set; }
  
  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

  public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
  public virtual ICollection<CampaignEvent> CampaignEvents { get; set; } = new List<CampaignEvent>();
  public virtual ICollection<Character> Characters { get; set; } = new List<Character>();
  public virtual User GameMaster { get; set; }
  public virtual GameSystem GameSystem { get; set; }
}
