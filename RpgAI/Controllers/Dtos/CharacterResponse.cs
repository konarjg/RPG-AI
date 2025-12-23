namespace RpgAI.Controllers.Dtos;

using Newtonsoft.Json.Linq;

public record CharacterResponse(Guid Id, string Name, string Overview, Guid CampaignId, JObject State);
