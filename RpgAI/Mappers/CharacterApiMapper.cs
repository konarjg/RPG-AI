namespace RpgAI.Mappers;

using Domain.Entities;
using RpgAI.Controllers.Dtos;
using Newtonsoft.Json.Linq;

public static class CharacterApiMapper
{

    public static CharacterResponse ToResponse(Character character)
    {
        return new CharacterResponse(character.Id, character.Name, character.Overview, character.CampaignId, JObject.Parse(character.State));
    }

    public static BrowseCharactersResponse ToBrowseResponse(Character character)
    {
        return new BrowseCharactersResponse(character.Id, character.Name, character.Overview, character.CampaignId);
    }
}
