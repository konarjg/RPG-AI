namespace Application.Dtos;

public record CreateCampaignCommand(Guid GameMasterUserId, Guid GameSystemId, string Name, string Overview);
