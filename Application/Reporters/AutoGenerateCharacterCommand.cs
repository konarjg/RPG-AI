namespace Application.Reporters;

using Domain.Entities;

public record AutoGenerateCharacterCommand(Campaign Campaign, GameSystem GameSystem, string? Concept = null);
