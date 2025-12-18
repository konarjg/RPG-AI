namespace Domain.Dtos;

public record CreateCampaignCommand(Guid OwnerId, string Title,  string Overview, Guid GameSystemId);
