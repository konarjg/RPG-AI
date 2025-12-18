namespace Infrastructure.Infrastructure.AiClient.Clients.Interfaces;

using Domain.Ports.Infrastructure.Dtos;

public interface ICharacterGenerationClient {
  Task<AiGenerateCharacterResponse> GenerateCharacterAsync(AiGenerateCharacterRequest request);
}
