using System.Collections.Generic;
using Application;
using Application.Dtos;
using Application.Exceptions;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Domain.Ports.Infrastructure.Dtos;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace UnitTests;

public class CharacterServiceTests {
  private readonly IAiClient _aiClient;
  private readonly ISchemaProvider _schemaProvider;
  private readonly ICampaignRepository _campaignRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly CharacterService _characterService;

  public CharacterServiceTests() {
    _aiClient = Substitute.For<IAiClient>();
    _schemaProvider = Substitute.For<ISchemaProvider>();
    _campaignRepository = Substitute.For<ICampaignRepository>();
    _unitOfWork = Substitute.For<IUnitOfWork>();

    _characterService = new CharacterService(_aiClient, _schemaProvider, _campaignRepository, _unitOfWork);
  }

  [Theory]
  [MemberData(nameof(GenerateCharacterAsyncTestData))]
  public async Task GenerateCharacterAsync_ShouldReturnCharacter_WhenCampaignExists(Campaign campaign, GenerateCharacterCommand command, AiCreateCharacterResponse aiResponse) {
      _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);
      _aiClient.CreateCharacterAsync(Arg.Any<AiCreateCharacterRequest>(), Arg.Any<CancellationToken>()).Returns(aiResponse);

      Character result = await _characterService.GenerateCharacterAsync(command);

      result.Should().NotBeNull();
      result.Name.Should().Be(aiResponse.Name);
      result.Overview.Should().Be(aiResponse.Overview);
      result.State.Should().BeEquivalentTo(aiResponse.State);
      result.CampaignId.Should().Be(campaign.Id);
      campaign.Characters.Should().Contain(result);
      await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
  }

  public static IEnumerable<object[]> GenerateCharacterAsyncTestData() {
      Guid campaignId = Guid.NewGuid();
      yield return new object[] {
          new Campaign {
              Id = campaignId,
              Name = "Test Campaign",
              Overview = "Test Overview",
              GameSystem = new GameSystem {
                  Id = Guid.NewGuid(),
                  Name = "D&D 5e",
                  CharacterSheetSchema = "{ \"type\": \"object\" }",
                  CharacterCreationGuide = "Create a character"
              },
              Characters = new List<Character>()
          },
          new GenerateCharacterCommand(campaignId, Guid.NewGuid(), "A brave warrior"),
          new AiCreateCharacterResponse {
              Name = "Sir Galahad",
              Overview = "A noble knight",
              State = new Dictionary<string, object> { { "strength", 18 } }
          }
      };
  }

  [Fact]
  public async Task GenerateCharacterAsync_ShouldThrowNotFoundException_WhenCampaignDoesNotExist() {
      GenerateCharacterCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "A brave warrior");
      _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns((Campaign)null);

      Func<Task> act = async () => await _characterService.GenerateCharacterAsync(command);

      await act.Should().ThrowAsync<NotFoundException<Campaign>>();
  }

  [Fact]
  public async Task GenerateCharacterAsync_ShouldThrowException_WhenSchemaValidationFails() {
      Campaign campaign = new Campaign {
          Id = Guid.NewGuid(),
          Name = "Test Campaign",
          Overview = "Test Overview",
          GameSystem = new GameSystem {
              Id = Guid.NewGuid(),
              Name = "D&D 5e",
              CharacterSheetSchema = "{ \"type\": \"object\" }",
              CharacterCreationGuide = "Create a character"
          },
          Characters = new List<Character>()
      };
      GenerateCharacterCommand command = new(campaign.Id, Guid.NewGuid(), "A brave warrior");
      AiCreateCharacterResponse aiResponse = new AiCreateCharacterResponse {
          Name = "Sir Galahad",
          Overview = "A noble knight",
          State = new Dictionary<string, object> { { "strength", 18 } }
      };

      _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);
      _aiClient.CreateCharacterAsync(Arg.Any<AiCreateCharacterRequest>(), Arg.Any<CancellationToken>()).Returns(aiResponse);
      _schemaProvider.When(x => x.ValidateWithSchema(aiResponse.State, campaign.GameSystem.CharacterSheetSchema))
          .Do(x => { throw new Exception("Schema validation failed"); });

      Func<Task> act = async () => await _characterService.GenerateCharacterAsync(command);

      await act.Should().ThrowAsync<Exception>().WithMessage("Schema validation failed");
  }
}
