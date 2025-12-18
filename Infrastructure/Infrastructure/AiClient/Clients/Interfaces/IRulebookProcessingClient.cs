namespace Infrastructure.Infrastructure.AiClient.Clients.Interfaces;

using Domain.Ports.Infrastructure.Dtos;

public interface IRulebookProcessingClient {
  Task<AiSplitRulebookResponse> SplitRulebookAsync(AiSplitRulebookRequest request);
}
