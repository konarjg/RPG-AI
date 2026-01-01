namespace Domain.Ports.Infrastructure;

using Dtos;

public interface IAiClient {
  Task<AiSplitRulebookResponse> SplitRulebookAsync(AiSplitRulebookRequest request);
  Task<AiEmbedTextResponse> EmbedTextAsync(AiEmbedTextRequest request);
  Task<List<AiEmbedTextResponse>> EmbedAllTextsAsync(List<AiEmbedTextRequest> request);
  Task<AiGenerateCharacterResponse> GenerateCharacterAsync(AiGenerateCharacterRequest request);
  Task<AiSummarizeSceneResponse> SummarizeSceneAsync(AiSummarizeSceneRequest request);
  Task<AiGenerateReactionResponse> GenerateReactionAsync(AiGenerateReactionRequest request);
  Task<AiInterpretActionResultsResponse> InterpretActionResultsAsync(AiInterpretActionResultsRequest request);
}
