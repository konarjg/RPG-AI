namespace Domain.Entities;

public class GameRuleChunk {
  public Guid Id { get; init; } = Guid.CreateVersion7();
  public Guid? GameSystemId { get; set; }
  public required string Content { get; set; }
  public required float[] ContentEmbedding { get; set; }
  
  
  public virtual GameSystem? GameSystem { get; set; }
}
