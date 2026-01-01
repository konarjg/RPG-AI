namespace Domain.Entities;

public class Message {
  public required Guid Id { get; set; }
  public required Guid SceneId { get; set; }
  public required DateTime SentAt { get; set; }
  public required string Content { get; set; }
  public required Role Sender { get; set; }
  
  public virtual Scene Scene { get; set; }
}

public enum Role {
  GameMaster,
  Player
}
