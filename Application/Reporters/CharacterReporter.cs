namespace Application.Reporters;

using Domain.Dtos;
using Domain.Entities;
using Domain.Factories.Interfaces;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Domain.Ports.Persistence;
using Exceptions;

public class CharacterReporter(ICharacterFactory characterFactory, IAiClient aiClient, ISchemaProvider schemaProvider, IRuleEngine ruleEngine) : ICharacterReporter {

  public async Task<Character> GenerateCharacterAsync(GenerateCharacterCommand command) {
    string schemaClasses = await schemaProvider.GenerateClassHierarchyFromSchemaAsync(command.GameSystem.CharacterSheetSchema);
    
    AiGenerateCharacterRequest request = new(
      command.Campaign.Overview,
      command.GameSystem.CharacterCreationGuide,
      schemaClasses,
      command.Concept);

    AiGenerateCharacterResponse response = await aiClient.GenerateCharacterAsync(request);
                                           
    RuleContext context = new() {
      CharacterSheet = "",
      CharacterSheetSchema = command.GameSystem.CharacterSheetSchema,
      CharacterCreationRule =  response.CharacterCreationRule,
      SchemaClasses = schemaClasses
    };
    
    RuleExecutionResult result = await ruleEngine.ExecuteRuleAsync(context);
    schemaProvider.ValidateContentWithSchema(result.FinalCharacterSheet, command.GameSystem.CharacterSheetSchema);

    Character character = characterFactory.CreateCharacter(new CreateCharacterCommand(command.Campaign.Id, response.Name,response.Overview,result.FinalCharacterSheet));
    
    return character;
  }
}
