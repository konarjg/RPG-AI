namespace Application.Reporters;

using Domain.Entities;

public record GenerateCharacterCommand(Campaign Campaign, GameSystem GameSystem, string? Concept = null);
