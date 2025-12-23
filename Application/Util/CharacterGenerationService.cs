namespace Application.Util;

using Domain.Dtos;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Interfaces;

public class CharacterGenerationService(IAiClient aiClient, ISchemaProvider schemaProvider, IRuleEngine ruleEngine) : ICharacterGenerationService {

  public async Task<CharacterGenerationResult> GenerateCharacterAsync(Campaign campaign,
    GameSystem gameSystem,
    string? concept = null) {
    
    string schemaClasses = await schemaProvider.GenerateClassHierarchyFromSchemaAsync(gameSystem.CharacterSheetSchema);
    
    AiGenerateCharacterRequest request = new(
      campaign.Overview,
      gameSystem.CharacterCreationGuide,
      schemaClasses,
      concept);

    AiGenerateCharacterResponse response = await aiClient.GenerateCharacterAsync(request);
                                           
    RuleContext context = new() {
      CharacterSheet = "",
      CharacterSheetSchema = gameSystem.CharacterSheetSchema,
      CharacterCreationRule =  response.CharacterCreationRule,
      SchemaClasses = schemaClasses
    };
    
    RuleExecutionResult result = await ruleEngine.ExecuteRuleAsync(context);
    schemaProvider.ValidateContentWithSchema(result.FinalCharacterSheet, gameSystem.CharacterSheetSchema);

    return new CharacterGenerationResult(result,response);
  }
}
