namespace Domain.Entities;

public class Scene
{
  public required Guid Id { get; set; }
  public required Guid SessionId { get; set; }
  public required int SceneNumber { get; set; }
  public string? Summary { get; set; }
  public float[]? SummaryEmbedding { get; set; }

  public virtual Session Session { get; set; }
}
