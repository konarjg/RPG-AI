namespace Domain.Entities;

public class User {
  public Guid Id { get; init; } = Guid.CreateVersion7();
  public required string Email { get; set; }
  public required string Username { get; set; }
  public required string HashedPassword { get; set; }
  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

  public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
