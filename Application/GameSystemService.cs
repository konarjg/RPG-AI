namespace Application;

using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Dtos;
using Interfaces;

public class GameSystemService(IAiClient aiClient, ISchemaProvider schemaProvider, IGameSystemRepository gameSystemRepository, IUnitOfWork unitOfWork) : IGameSystemService {

  public async Task<GameSystem?> GetSystemByIdAsync(Guid id,
    CancellationToken cancellationToken = default) {

    return await gameSystemRepository.GetByIdAsync(id,cancellationToken);
  }

  public async Task<CursorResult<GameSystem>> BrowseSystemsAsync(string? cursor,  
    int pageSize,
    CancellationToken cancellationToken = default) {
    
    return await gameSystemRepository.GetAllAsync(cursor,pageSize,cancellationToken);
  }

  public async Task<GameSystem> UploadSystemAsync(UploadGameSystemCommand command,
    CancellationToken cancellationToken = default) {

    ExtractedRules extractedRules = await ExtractRulesAsync(command.RulebookStream,cancellationToken);
    
    GameSystem system = new() {
      Title = command.Title,
      Overview = command.Overview,
      CharacterSheetSchema = await schemaProvider.LoadSchemaAsync(command.CharacterSheetSchemaStream,cancellationToken),
      Rules = extractedRules.Rules,
      CharacterCreationGuide = extractedRules.CharacterCreationGuide
    };
    
    gameSystemRepository.Add(system);
    await unitOfWork.SaveChangesAsync(cancellationToken);
    
    return system;
  }

  private async Task<ExtractedRules> ExtractRulesAsync(Stream rulebookStream, CancellationToken cancellationToken = default) {
    AiRulebookChunkResponse chunkResponse = await aiClient.ChunkRulebookAsync(rulebookStream,cancellationToken);
    List<RulebookChunk> chunks = chunkResponse.Chunks.ToList();
    List<GameRuleChunk> rules = new(chunks.Count);

    foreach (string chunk in chunks.Select(c => c.Content)) {
      rules.Add(new GameRuleChunk() {
        Content = chunk,
        ContentEmbedding = await aiClient.EmbedTextAsync(chunk,cancellationToken)
      });
    }

    return new ExtractedRules(
      rules,
      string.Join("\n\n", chunks.Where(c => c.Tag is RulebookChunkTag.CharacterCreation).Select(c => c.Content))
    );
  }

  private record ExtractedRules(ICollection<GameRuleChunk> Rules,string CharacterCreationGuide);
}
