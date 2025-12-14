namespace Domain.Ports.Infrastructure.Dtos;

public record AiSplitRulebookResponse(List<RulebookSplitEntry> Entries);
public record RulebookSplitEntry(string Content, string Summary);
 