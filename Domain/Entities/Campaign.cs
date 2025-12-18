namespace Domain.Entities;

public class Campaign {
  public required Guid Id { get; set; }
  public required Guid OwnerId { get; set; }
  public required string Title { get; set; }
  public required string Overview { get; set; }
  public required Guid GameSystemId { get; set; }
  public required DateTime StartedAt { get; set; }
  
  public virtual User Owner { get; set; }
  public virtual GameSystem GameSystem { get; set; }
  public virtual List<Session> Sessions { get; set; } = new();
  public virtual List<Character> Characters { get; set; } = new();
}
