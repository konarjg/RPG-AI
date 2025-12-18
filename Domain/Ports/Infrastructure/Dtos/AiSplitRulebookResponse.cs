namespace Domain.Ports.Infrastructure.Dtos;

public record AiSplitRulebookResponse(string CharacterCreationGuide, List<RulebookSplitEntry> Entries);
public record RulebookSplitEntry(string Content, string Summary);
 