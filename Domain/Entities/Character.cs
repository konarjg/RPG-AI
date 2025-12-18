namespace Domain.Entities;

public class Character {
  public required Guid Id { get; set; }
  public required string Name { get; set; }
  public required string Overview { get; set; }
  public required Guid CampaignId { get; set; }
  public required string State { get; set; }
  
  public virtual Campaign Campaign { get; set; }
}
