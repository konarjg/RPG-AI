namespace Infrastructure.Diagnostics;

using System.Diagnostics;

public static class RpgAiActivitySource {
  public static readonly ActivitySource Instance = new("RpgAI");
}
