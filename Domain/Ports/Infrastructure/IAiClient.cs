namespace Domain.Ports.Infrastructure;

using Dtos;

public interface IAiClient {
  Task<AiSplitRulebookResponse> SplitRulebookAsync(AiSplitRulebookRequest request);
  Task<AiEmbedTextResponse> EmbedTextAsync(AiEmbedTextRequest request);
  Task<List<AiEmbedTextResponse>> EmbedAllTextsAsync(List<AiEmbedTextRequest> request);
  Task<AiGenerateCharacterResponse> GenerateCharacterAsync(AiGenerateCharacterRequest request);
}
