namespace Domain.Entities;

public class Session
{
  public required Guid Id { get; set; }
  public required Guid CampaignId { get; set; }
  public required int SessionNumber { get; set; }
  public int? LastSceneNumber { get; set; }
  public Guid? CurrentSceneId { get; set; }
  public string? Summary { get; set; }
  public float[]? SummaryEmbedding { get; set; }

  public virtual Campaign Campaign { get; set; }
  public virtual Scene? CurrentScene { get; set; }
  public virtual List<Scene> Scenes { get; set; } = new();
}
