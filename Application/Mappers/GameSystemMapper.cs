namespace Application.Mappers;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Infrastructure.Dtos;


public static class GameSystemMapper
{

  public static CreateGameSystemCommand ToCommand(string title, string overview, Guid userId, string characterSheetSchema, AiSplitRulebookResponse splitResponse, List<AiEmbedTextResponse> embeddings)
  {
    if (splitResponse.Entries.Count != embeddings.Count)
    {
      throw new InvalidOperationException($"Embedding mismatch! Sent {splitResponse.Entries.Count} summaries but received {embeddings.Count} embeddings.");
    }

    List<CreateRulebookChapterCommand> chapters = splitResponse.Entries
       .Zip(embeddings)
       .Select(g => new CreateRulebookChapterCommand(g.First.Content, g.First.Summary, g.Second.Embedding))
       .ToList();

    return new CreateGameSystemCommand(title, overview, userId, characterSheetSchema, splitResponse.CharacterCreationGuide, chapters);
  }

}
