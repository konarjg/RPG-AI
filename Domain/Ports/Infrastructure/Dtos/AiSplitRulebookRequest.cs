namespace Domain.Ports.Infrastructure.Dtos;

public record AiSplitRulebookRequest(Stream RulebookStream, string RulebookContentType);
