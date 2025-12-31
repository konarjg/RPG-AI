namespace Infrastructure.Options;

public record AiClientOptions {
  public RulebookAgent RulebookAgent { get; init; }
  public EmbeddingAgent EmbeddingAgent { get; init; }
  public CharacterGenerationAgent CharacterGenerationAgent { get; init; }
}

public record TracingOptions {
  public string PublicKey { get; init; }
  public string SecretKey { get; init; }
  public string Host { get; init; } = "https://cloud.langfuse.com";
}

public record RulebookAgent {
  public string Model { get; init; }
  public string Prompt { get; init; }
  public string ApiKey { get; init; }
}

public record EmbeddingAgent {
  public string Model { get; init; }
  public int Dimensions { get; init; }
  public string BaseUrl { get; init; }
  public string ApiKey { get; init; }
}

public record CharacterGenerationAgent {
  public string Model { get; init; }
  public string BaseUrl { get; init; }
  public string ApiKey { get; init; }
  public string Prompt  { get; init; }
}
