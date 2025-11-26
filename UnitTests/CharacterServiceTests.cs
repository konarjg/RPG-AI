using Application;
using Application.Dtos;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using Moq;
using Xunit;
using Application.Exceptions;

namespace UnitTests;

public class CharacterServiceTests {
  private readonly Mock<IAiClient> _aiClientMock = new();
  private readonly Mock<ISchemaProvider> _schemaProviderMock = new();
  private readonly Mock<ICampaignRepository> _campaignRepositoryMock = new();
  private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
  private readonly CharacterService _characterService;

  public CharacterServiceTests() {
    _characterService = new CharacterService(
      _aiClientMock.Object,
      _schemaProviderMock.Object,
      _campaignRepositoryMock.Object,
      _unitOfWorkMock.Object
    );
  }

  public static IEnumerable<object?[]> GetGenerateCharacterTestData() {
    yield return new object?[] {
      "Test Name", "Test Overview", new Dictionary<string, object> { { "key", "value" } }, "Test Concept"
    };
    yield return new object?[] {
      "Another Name", "Another Overview", new Dictionary<string, object> { { "attr", 123 } }, null
    };
  }

  [Theory]
  [MemberData(nameof(GetGenerateCharacterTestData))]
  public async Task GenerateCharacterAsync_ShouldReturnCharacter_WhenCampaignExists(
    string name, string overview, Dictionary<string, object> state, string? concept) {
    GenerateCharacterCommand command = new(Guid.NewGuid(), Guid.NewGuid(), concept);
    Campaign campaign = new() {
      Id = command.CampaignId,
      Name = "Test Campaign",
      Overview = "Test Overview",
      GameSystem = new GameSystem {
        Name = "Test Game System",
        CharacterSheetSchema = "{}"
      }
    };
    AiCreateCharacterResponse aiResponse = new(name, overview, state);

    _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(campaign);
    _aiClientMock.Setup(c => c.CreateCharacterAsync(It.IsAny<AiCreateCharacterRequest>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(aiResponse);

    Character result = await _characterService.GenerateCharacterAsync(command);

    Assert.NotNull(result);
    Assert.Equal(aiResponse.Name, result.Name);
    Assert.Equal(aiResponse.Overview, result.Overview);
    Assert.Equal(aiResponse.State, result.State);
    Assert.Contains(result, campaign.Characters);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GenerateCharacterAsync_ShouldThrowNotFoundException_WhenCampaignNotFound() {
    GenerateCharacterCommand command = new(Guid.NewGuid(), Guid.NewGuid());

    _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Campaign)null!);

    await Assert.ThrowsAsync<NotFoundException<Campaign>>(() => _characterService.GenerateCharacterAsync(command));
  }

  [Fact]
  public async Task GenerateCharacterAsync_ShouldThrowJsonSchemaValidationException_WhenSchemaIsInvalid() {
    GenerateCharacterCommand command = new(Guid.NewGuid(), Guid.NewGuid());
    Campaign campaign = new() {
      Id = command.CampaignId,
      Name = "Test Campaign",
      Overview = "Test Overview",
      GameSystem = new GameSystem {
        Name = "Test Game System",
        CharacterSheetSchema = "{}"
      }
    };
    AiCreateCharacterResponse aiResponse = new("a", "b", new Dictionary<string, object>());

    _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(campaign);
    _aiClientMock.Setup(c => c.CreateCharacterAsync(It.IsAny<AiCreateCharacterRequest>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(aiResponse);
    _schemaProviderMock
      .Setup(p => p.ValidateWithSchema(aiResponse.State, campaign.GameSystem.CharacterSheetSchema))
      .Throws(new JsonSchemaValidationException("Invalid schema"));

    await Assert.ThrowsAsync<JsonSchemaValidationException>(() => _characterService.GenerateCharacterAsync(command));
  }
}