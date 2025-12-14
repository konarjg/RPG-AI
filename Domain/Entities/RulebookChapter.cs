namespace Domain.Entities;

public class RulebookChapter {
  public required Guid Id { get; set; } 
  public required Guid GameSystemId { get; set; }
  public required string Content { get; set; }
  public required string Summary { get; set; }
  public required float[] SummaryEmbedding { get; set; }
  
  public virtual GameSystem GameSystem { get; set; }
}
