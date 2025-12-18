namespace Domain.Dtos;

public record CreateCharacterCommand(Guid CampaignId, string Name, string Overview, string InitialState);
