namespace Domain.Entities;

public class User {
  public required Guid Id { get; set; }

  public virtual List<Campaign> Campaigns { get; set; } = new();
}
