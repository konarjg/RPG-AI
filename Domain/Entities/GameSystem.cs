namespace Domain.Entities;

public class GameSystem {
  public Guid Id { get; init; } = Guid.CreateVersion7();
  public required string Title { get; set; }
  public required string Overview { get; set; }
  public required string CharacterCreationGuide { get; set; }
  public DateTime UploadDate { get; init; } = DateTime.UtcNow;
  public required Dictionary<string, object> CharacterSheetSchema { get; set; }
  
  public virtual ICollection<GameRuleChunk> Rules { get; set; } = new List<GameRuleChunk>();
}
