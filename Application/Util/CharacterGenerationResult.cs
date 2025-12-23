namespace Application.Util;

using Domain.Ports.Infrastructure.Dtos;

public record CharacterGenerationResult(RuleExecutionResult RuleExecutionResult,AiGenerateCharacterResponse GenerateCharacterResponse);
