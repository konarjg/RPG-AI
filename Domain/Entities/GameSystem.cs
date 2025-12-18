namespace Domain.Entities;

public class GameSystem {
    public required Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public required string Title { get; set; }
    public required string Overview { get; set; }
    public required DateTime UploadedAt { get; set; } 
    public required string CharacterSheetSchema { get; set; }
    public required string CharacterCreationGuide { get; set; }

    public virtual List<RulebookChapter> Chapters { get; set; } = new();
    public virtual List<Campaign> Campaigns { get; set; } = new();

    public bool IsPublic => !OwnerId.HasValue;
}
