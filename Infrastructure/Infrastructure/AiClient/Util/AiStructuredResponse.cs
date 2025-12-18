namespace Infrastructure.Infrastructure.AiClient.Util;

using Newtonsoft.Json;

public record AiStructuredResponse {
  [JsonProperty("choices")]
  public ICollection<Choice> Choices { get; init; }
}

public record Choice {
  [JsonProperty("message")]
  public Message Message { get; init; }
}

public record Message {
  [JsonProperty("reasoning")]
  public string? Reasoning { get; init; }
}