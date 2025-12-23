namespace RpgAI.Controllers;

public static class GameSystemRoutes {
  public const string Base = "api/game-systems";
  public const string Get = "{id}";
  public const string Browse = "";
  public const string Upload = "";
}

public static class CampaignRoutes {
  public const string Base = "api/campaigns";
  public const string Get = "{id}";
  public const string Browse = "";
  public const string Start = "";
}

public static class CharacterRoutes {
  public const string Base = $"{CampaignRoutes.Base}/{{campaignId}}/characters";
  public const string Get = "{id}";
  public const string Browse = "";
  public const string Generate = "";
}