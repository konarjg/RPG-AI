namespace Domain.Entities;

public class Character {
  public Guid Id { get; init; } = Guid.CreateVersion7();
  public Guid? CampaignId { get; set; }
  public required string Name { get; set; }
  public required string Overview { get; set; }
  public required Dictionary<string, object> State { get; set; }
  public required CharacterType CharacterType { get; set; }
  
  public virtual Campaign? Campaign { get; set; }
}
