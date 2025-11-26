using Application;
using Application.Configuration;
using Application.Dtos;
using Application.Exceptions;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using FluentAssertions;
using NSubstitute;

namespace UnitTests;

public class GameplayServiceTests
{
  private readonly ICampaignRepository _campaignRepository = Substitute.For<ICampaignRepository>();
  private readonly IEmbeddingRepository _embeddingRepository = Substitute.For<IEmbeddingRepository>();
  private readonly IEventBroadcaster _eventBroadcaster = Substitute.For<IEventBroadcaster>();
  private readonly IRuleEngine _ruleEngine = Substitute.For<IRuleEngine>();
  private readonly IAiClient _aiClient = Substitute.For<IAiClient>();
  private readonly ISchemaProvider _schemaProvider = Substitute.For<ISchemaProvider>();
  private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
  private readonly AiConfiguration _aiConfiguration = new();

  private readonly GameplayService _sut;

  public GameplayServiceTests()
  {
    _sut = new GameplayService(
      _campaignRepository,
      _embeddingRepository,
      _eventBroadcaster,
      _ruleEngine,
      _aiClient,
      _schemaProvider,
      _unitOfWork,
      _aiConfiguration
    );
  }

  [Theory]
  [MemberData(nameof(StartSessionAsyncSuccessCases))]
  public async Task StartSessionAsync_ShouldCreateNewSession_WhenCampaignExistsAndNoActiveSession(
    StartSessionCommand command, Campaign campaign)
  {
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);

    Session result = await _sut.StartSessionAsync(command);

    result.Should().NotBeNull();
    result.Title.Should().Be(command.Title);
    result.CampaignId.Should().Be(command.CampaignId);
    result.Status.Should().Be(SessionStatus.Active);
    campaign.Sessions.Should().Contain(result);
    await _unitOfWork.Received(1).SaveChangesAsync();
  }

  public static IEnumerable<object[]> StartSessionAsyncSuccessCases()
  {
    Guid userId = Guid.NewGuid();
    Guid campaignId = Guid.NewGuid();

    yield return new object[]
    {
      new StartSessionCommand(userId, campaignId, "New Session"),
      new Campaign
      {
        Id = campaignId,
        GameMasterUserId = userId,
        GameSystemId = Guid.NewGuid(),
        Name = "Test Campaign",
        Overview = "Test Overview",
        Sessions = new List<Session>()
      }
    };
  }

  [Fact]
  public async Task StartSessionAsync_ShouldThrowNotFoundException_WhenCampaignNotFound()
  {
    StartSessionCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "New Session");
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns((Campaign)null);

    Func<Task> act = async () => await _sut.StartSessionAsync(command);

    await act.Should().ThrowExactlyAsync<NotFoundException<Campaign>>();
  }

  [Fact]
  public async Task StartSessionAsync_ShouldThrowAlreadyExistsException_WhenSessionExists()
  {
    StartSessionCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "New Session");
    Campaign campaign = new()
    {
      Id = command.CampaignId,
      GameMasterUserId = command.UserId,
      GameSystemId = Guid.NewGuid(),
      Name = "Test Campaign",
      Overview = "Test Overview",
      Sessions = new List<Session>
      {
        new()
        {
          Id = Guid.NewGuid(),
          CampaignId = command.CampaignId,
          Title = "Active Session",
          Status = SessionStatus.Active,
          SessionNumber = 1
        }
      }
    };
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);

    Func<Task> act = async () => await _sut.StartSessionAsync(command);

    await act.Should().ThrowExactlyAsync<AlreadyExistsException<Session>>();
  }

  [Theory]
  [MemberData(nameof(ProcessTurnAsyncSuccessCases))]
  public async Task ProcessTurnAsync_ShouldProcessTurn_WhenCampaignAndActiveSessionExist(
    ProcessTurnCommand command, Campaign campaign)
  {
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);
    _aiClient.EmbedTextAsync(Arg.Any<string>()).Returns(new float[] { 1, 2, 3 });
    _aiClient.ReactToEventAsync(Arg.Any<AiActionRequest>()).Returns(new AiActionResponse("AI Response", null));

    await _sut.ProcessTurnAsync(command);

    await _eventBroadcaster.Received(1).BroadcastEventAsync(Arg.Any<CampaignEvent>(), Arg.Any<CancellationToken>());
    campaign.CampaignEvents.Count.Should().Be(2);
    await _unitOfWork.Received(1).SaveChangesAsync();
  }

  public static IEnumerable<object[]> ProcessTurnAsyncSuccessCases()
  {
    Guid userId = Guid.NewGuid();
    Guid campaignId = Guid.NewGuid();
    Guid characterId = Guid.NewGuid();

    yield return new object[]
    {
      new ProcessTurnCommand(userId, campaignId, "Test Message", characterId, DateTime.UtcNow),
      new Campaign
      {
        Id = campaignId,
        GameMasterUserId = userId,
        GameSystemId = Guid.NewGuid(),
        Name = "Test Campaign",
        Overview = "Test Overview",
        Sessions = new List<Session>
        {
          new()
          {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Title = "Active Session",
            Status = SessionStatus.Active,
            SessionNumber = 1
          }
        },
        Characters = new List<Character>
        {
          new()
          {
            Id = characterId,
            CampaignId = campaignId,
            Name = "Test Character",
            Description = "Test Description",
            CharacterType = CharacterType.Player,
            State = new Dictionary<string, object>()
          }
        }
      }
    };
  }

  [Fact]
  public async Task ProcessTurnAsync_ShouldThrowNotFoundException_WhenCampaignNotFound()
  {
    ProcessTurnCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "Test Message", Guid.NewGuid(), DateTime.UtcNow);
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns((Campaign)null);

    Func<Task> act = async () => await _sut.ProcessTurnAsync(command);

    await act.Should().ThrowExactlyAsync<NotFoundException<Campaign>>();
  }

  [Fact]
  public async Task ProcessTurnAsync_ShouldThrowNotFoundException_WhenActiveSessionNotFound()
  {
    ProcessTurnCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "Test Message", Guid.NewGuid(), DateTime.UtcNow);
    Campaign campaign = new()
    {
      Id = command.CampaignId,
      GameMasterUserId = command.UserId,
      GameSystemId = Guid.NewGuid(),
      Name = "Test Campaign",
      Overview = "Test Overview",
      Sessions = new List<Session>()
    };
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);

    Func<Task> act = async () => await _sut.ProcessTurnAsync(command);

    await act.Should().ThrowExactlyAsync<NotFoundException<Session>>();
  }

  [Fact]
  public async Task ProcessTurnAsync_ShouldThrowNotFoundException_WhenCharacterNotFound()
  {
    ProcessTurnCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "Test Message", Guid.NewGuid(), DateTime.UtcNow);
    Campaign campaign = new()
    {
      Id = command.CampaignId,
      GameMasterUserId = command.UserId,
      GameSystemId = Guid.NewGuid(),
      Name = "Test Campaign",
      Overview = "Test Overview",
      Sessions = new List<Session>
      {
        new()
        {
          Id = Guid.NewGuid(),
          CampaignId = command.CampaignId,
          Title = "Active Session",
          Status = SessionStatus.Active,
          SessionNumber = 1
        }
      },
      Characters = new List<Character>()
    };
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);

    Func<Task> act = async () => await _sut.ProcessTurnAsync(command);

    await act.Should().ThrowExactlyAsync<NotFoundException<Character>>();
  }

  [Theory]
  [MemberData(nameof(EndSessionAsyncSuccessCases))]
  public async Task EndSessionAsync_ShouldEndSession_WhenCampaignAndActiveSessionExist(
    EndSessionCommand command, Campaign campaign)
  {
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);
    _aiClient.SummarizeSessionAsync(Arg.Any<Session>()).Returns("Session Summary");
    _aiClient.EmbedTextAsync(Arg.Any<string>()).Returns(new float[] { 1, 2, 3 });

    await _sut.EndSessionAsync(command);

    Session activeSession = campaign.Sessions.First();
    activeSession.Status.Should().Be(SessionStatus.Completed);
    activeSession.Summary.Should().NotBeNullOrEmpty();
    activeSession.SummaryEmbedding.Should().NotBeNullOrEmpty();
    await _unitOfWork.Received(1).SaveChangesAsync();
  }

  public static IEnumerable<object[]> EndSessionAsyncSuccessCases()
  {
    Guid userId = Guid.NewGuid();
    Guid campaignId = Guid.NewGuid();

    yield return new object[]
    {
      new EndSessionCommand(userId, campaignId),
      new Campaign
      {
        Id = campaignId,
        GameMasterUserId = userId,
        GameSystemId = Guid.NewGuid(),
        Name = "Test Campaign",
        Overview = "Test Overview",
        Sessions = new List<Session>
        {
          new()
          {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Title = "Active Session",
            Status = SessionStatus.Active,
            SessionNumber = 1
          }
        }
      }
    };
  }

  [Fact]
  public async Task EndSessionAsync_ShouldThrowNotFoundException_WhenCampaignNotFound()
  {
    EndSessionCommand command = new(Guid.NewGuid(), Guid.NewGuid());
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns((Campaign)null);

    Func<Task> act = async () => await _sut.EndSessionAsync(command);

    await act.Should().ThrowExactlyAsync<NotFoundException<Campaign>>();
  }

  [Fact]
  public async Task EndSessionAsync_ShouldThrowNotFoundException_WhenActiveSessionNotFound()
  {
    EndSessionCommand command = new(Guid.NewGuid(), Guid.NewGuid());
    Campaign campaign = new()
    {
      Id = command.CampaignId,
      GameMasterUserId = command.UserId,
      GameSystemId = Guid.NewGuid(),
      Name = "Test Campaign",
      Overview = "Test Overview",
      Sessions = new List<Session>()
    };
    _campaignRepository.GetByIdAsync(command.CampaignId, command.UserId).Returns(campaign);

    Func<Task> act = async () => await _sut.EndSessionAsync(command);

    await act.Should().ThrowExactlyAsync<NotFoundException<Session>>();
  }
}
