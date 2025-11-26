using Application;
using Application.Configuration;
using Application.Dtos;
using Application.Exceptions;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Domain.Ports.Infrastructure.Dtos;
using System.Linq;

namespace UnitTests {

  public class GameplayServiceTests {
    private readonly Mock<ICampaignRepository> _campaignRepositoryMock;
    private readonly Mock<IEmbeddingRepository> _embeddingRepositoryMock;
    private readonly Mock<IEventBroadcaster> _eventBroadcasterMock;
    private readonly Mock<IRuleEngine> _ruleEngineMock;
    private readonly Mock<IAiClient> _aiClientMock;
    private readonly Mock<ISchemaProvider> _schemaProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AiConfiguration _aiConfiguration;
    private readonly GameplayService _gameplayService;

    public GameplayServiceTests() {
      _campaignRepositoryMock = new Mock<ICampaignRepository>();
      _embeddingRepositoryMock = new Mock<IEmbeddingRepository>();
      _eventBroadcasterMock = new Mock<IEventBroadcaster>();
      _ruleEngineMock = new Mock<IRuleEngine>();
      _aiClientMock = new Mock<IAiClient>();
      _schemaProviderMock = new Mock<ISchemaProvider>();
      _unitOfWorkMock = new Mock<IUnitOfWork>();
      _aiConfiguration = new AiConfiguration { RecentEventsPageSize = 10, RulesRetrievalLimit = 5, SessionSummariesRetrievalLimit = 3, EventsRetrievalLimit = 20 };
      _gameplayService = new GameplayService(
          _campaignRepositoryMock.Object,
          _embeddingRepositoryMock.Object,
          _eventBroadcasterMock.Object,
          _ruleEngineMock.Object,
          _aiClientMock.Object,
          _schemaProviderMock.Object,
          _unitOfWorkMock.Object,
          _aiConfiguration);
    }

    public static IEnumerable<object[]> StartSessionAsync_ShouldStartSession_WhenNoActiveSessionExists_Data() {
      yield return new object[] { new StartSessionCommand(Guid.NewGuid(), Guid.NewGuid(), "New Session") };
      yield return new object[] { new StartSessionCommand(Guid.NewGuid(), Guid.NewGuid(), "Another Session") };
    }

    [Theory]
    [MemberData(nameof(StartSessionAsync_ShouldStartSession_WhenNoActiveSessionExists_Data))]
    public async Task StartSessionAsync_ShouldStartSession_WhenNoActiveSessionExists(StartSessionCommand command) {
      Campaign campaign = new Campaign { Id = command.CampaignId, UserId = command.UserId, Sessions = new List<Session>() };
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync(campaign);

      Session result = await _gameplayService.StartSessionAsync(command);

      Assert.NotNull(result);
      Assert.Equal(command.Title, result.Title);
      Assert.Single(campaign.Sessions);
      Assert.Equal(SessionStatus.Active, campaign.Sessions.First().Status);
    }

    public static IEnumerable<object[]> StartSessionAsync_ShouldThrowNotFoundException_WhenCampaignNotFound_Data() {
      yield return new object[] { new StartSessionCommand(Guid.NewGuid(), Guid.NewGuid(), "New Session") };
    }

    [Theory]
    [MemberData(nameof(StartSessionAsync_ShouldThrowNotFoundException_WhenCampaignNotFound_Data))]
    public async Task StartSessionAsync_ShouldThrowNotFoundException_WhenCampaignNotFound(StartSessionCommand command) {
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync((Campaign)null);

      await Assert.ThrowsAsync<NotFoundException<Campaign>>(() => _gameplayService.StartSessionAsync(command));
    }

    public static IEnumerable<object[]> StartSessionAsync_ShouldThrowAlreadyExistsException_WhenActiveSessionExists_Data() {
      yield return new object[] { new StartSessionCommand(Guid.NewGuid(), Guid.NewGuid(), "New Session") };
    }

    [Theory]
    [MemberData(nameof(StartSessionAsync_ShouldThrowAlreadyExistsException_WhenActiveSessionExists_Data))]
    public async Task StartSessionAsync_ShouldThrowAlreadyExistsException_WhenActiveSessionExists(StartSessionCommand command) {
      Campaign campaign = new Campaign { Id = command.CampaignId, UserId = command.UserId, Sessions = new List<Session> { new Session { Status = SessionStatus.Active } } };
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync(campaign);

      await Assert.ThrowsAsync<AlreadyExistsException<Session>>(() => _gameplayService.StartSessionAsync(command));
    }

    public static IEnumerable<object[]> ProcessTurnAsync_ShouldProcessTurn_WhenCampaignAndActiveSessionExists_Data() {
      yield return new object[] { new ProcessTurnCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "User Action", new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)) };
      yield return new object[] { new ProcessTurnCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Another User Action", new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)) };
    }

    [Theory]
    [MemberData(nameof(ProcessTurnAsync_ShouldProcessTurn_WhenCampaignAndActiveSessionExists_Data))]
    public async Task ProcessTurnAsync_ShouldProcessTurn_WhenCampaignAndActiveSessionExists(ProcessTurnCommand command) {
      Character character = new Character { Id = command.NextCharacterId, State = new Dictionary<string, object>() };
      Campaign campaign = new Campaign {
        Id = command.CampaignId,
        UserId = command.UserId,
        GameSystem = new GameSystem { CharacterSheetSchema = new Dictionary<string, object>() },
        Sessions = new List<Session> { new Session { Id = Guid.NewGuid(), Status = SessionStatus.Active } },
        Characters = new List<Character> { character },
        CampaignEvents = new List<CampaignEvent>()
      };

      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync(campaign);
      _campaignRepositoryMock.Setup(r => r.GetEventsAsync(campaign.Id, campaign.Sessions[0].Id, command.UserId, null, _aiConfiguration.RecentEventsPageSize, default))
          .ReturnsAsync(new CursorResult<CampaignEvent>(new List<CampaignEvent>(), null));
      _aiClientMock.Setup(c => c.GenerateRetrievalQueryEmbeddingAsync(It.IsAny<string>(), It.IsAny<List<CampaignEvent>>(), default)).ReturnsAsync(new float[] { 1.0f });
      _embeddingRepositoryMock.Setup(r => r.FindRelevantRulesAsync(It.IsAny<Guid>(), It.IsAny<float[]>(), It.IsAny<int>(), default)).ReturnsAsync(new List<GameRuleChunk>());
      _embeddingRepositoryMock.Setup(r => r.FindRelevantSessionSummariesAsync(It.IsAny<Guid>(), It.IsAny<float[]>(), It.IsAny<int>(), default)).ReturnsAsync(new List<Session>());
      _embeddingRepositoryMock.Setup(r => r.FindRelevantHistoryAsync(It.IsAny<Guid>(), It.IsAny<float[]>(), It.IsAny<int>(), default)).ReturnsAsync(new List<CampaignEvent>());
      _aiClientMock.Setup(c => c.ReactToEventAsync(It.IsAny<AiActionRequest>(), default)).ReturnsAsync(new AiActionResponse { Content = "AI Response" });
      _aiClientMock.Setup(c => c.EmbedTextAsync(It.IsAny<string>(), default)).ReturnsAsync(new float[] { 1.0f });

      await _gameplayService.ProcessTurnAsync(command);

      _eventBroadcasterMock.Verify(b => b.BroadcastEventAsync(It.Is<CampaignEvent>(e => e.Content == "AI Response"), default), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ProcessTurnAsync_ShouldExecuteRule_WhenResponseHasAction() {
      ProcessTurnCommand command = new ProcessTurnCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "User Action", new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
      Character character = new Character { Id = command.NextCharacterId, State = new Dictionary<string, object>() };
      Campaign campaign = new Campaign {
        Id = command.CampaignId,
        UserId = command.UserId,
        GameSystem = new GameSystem { CharacterSheetSchema = new Dictionary<string, object>() },
        Sessions = new List<Session> { new Session { Id = Guid.NewGuid(), Status = SessionStatus.Active } },
        Characters = new List<Character> { character },
        CampaignEvents = new List<CampaignEvent>()
      };
      AiActionResponse response = new AiActionResponse { Content = "AI Response", Action = new Dictionary<string, object>() };

      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync(campaign);
      _campaignRepositoryMock.Setup(r => r.GetEventsAsync(campaign.Id, campaign.Sessions[0].Id, command.UserId, null, _aiConfiguration.RecentEventsPageSize, default))
          .ReturnsAsync(new CursorResult<CampaignEvent>(new List<CampaignEvent>(), null));
      _aiClientMock.Setup(c => c.GenerateRetrievalQueryEmbeddingAsync(It.IsAny<string>(), It.IsAny<List<CampaignEvent>>(), default)).ReturnsAsync(new float[] { 1.0f });
      _embeddingRepositoryMock.Setup(r => r.FindRelevantRulesAsync(It.IsAny<Guid>(), It.IsAny<float[]>(), It.IsAny<int>(), default)).ReturnsAsync(new List<GameRuleChunk>());
      _embeddingRepositoryMock.Setup(r => r.FindRelevantSessionSummariesAsync(It.IsAny<Guid>(), It.IsAny<float[]>(), It.IsAny<int>(), default)).ReturnsAsync(new List<Session>());
      _embeddingRepositoryMock.Setup(r => r.FindRelevantHistoryAsync(It.IsAny<Guid>(), It.IsAny<float[]>(), It.IsAny<int>(), default)).ReturnsAsync(new List<CampaignEvent>());
      _aiClientMock.Setup(c => c.ReactToEventAsync(It.IsAny<AiActionRequest>(), default)).ReturnsAsync(response);
      _aiClientMock.Setup(c => c.EmbedTextAsync(It.IsAny<string>(), default)).ReturnsAsync(new float[] { 1.0f });

      await _gameplayService.ProcessTurnAsync(command);

      _ruleEngineMock.Verify(r => r.ExecuteRule(character, response.Action), Times.Once);
    }

    public static IEnumerable<object[]> ProcessTurnAsync_ShouldThrowNotFoundException_WhenCampaignNotFound_Data() {
      yield return new object[] { new ProcessTurnCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "User Action", new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)) };
    }

    [Theory]
    [MemberData(nameof(ProcessTurnAsync_ShouldThrowNotFoundException_WhenCampaignNotFound_Data))]
    public async Task ProcessTurnAsync_ShouldThrowNotFoundException_WhenCampaignNotFound(ProcessTurnCommand command) {
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync((Campaign)null);

      await Assert.ThrowsAsync<NotFoundException<Campaign>>(() => _gameplayService.ProcessTurnAsync(command));
    }

    public static IEnumerable<object[]> ProcessTurnAsync_ShouldThrowNotFoundException_WhenNoActiveSession_Data() {
      yield return new object[] { new ProcessTurnCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "User Action", new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)) };
    }

    [Theory]
    [MemberData(nameof(ProcessTurnAsync_ShouldThrowNotFoundException_WhenNoActiveSession_Data))]
    public async Task ProcessTurnAsync_ShouldThrowNotFoundException_WhenNoActiveSession(ProcessTurnCommand command) {
      Campaign campaign = new Campaign { Id = command.CampaignId, UserId = command.UserId, Sessions = new List<Session>() };
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync(campaign);

      await Assert.ThrowsAsync<NotFoundException<Session>>(() => _gameplayService.ProcessTurnAsync(command));
    }

    public static IEnumerable<object[]> ProcessTurnAsync_ShouldThrowNotFoundException_WhenCharacterNotFound_Data() {
      yield return new object[] { new ProcessTurnCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "User Action", new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)) };
    }

    [Theory]
    [MemberData(nameof(ProcessTurnAsync_ShouldThrowNotFoundException_WhenCharacterNotFound_Data))]
    public async Task ProcessTurnAsync_ShouldThrowNotFoundException_WhenCharacterNotFound(ProcessTurnCommand command) {
      Campaign campaign = new Campaign {
        Id = command.CampaignId,
        UserId = command.UserId,
        Sessions = new List<Session> { new Session { Status = SessionStatus.Active } },
        Characters = new List<Character>()
      };
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync(campaign);
      _campaignRepositoryMock.Setup(r => r.GetEventsAsync(campaign.Id, It.IsAny<Guid>(), command.UserId, null, _aiConfiguration.RecentEventsPageSize, default))
          .ReturnsAsync(new CursorResult<CampaignEvent>(new List<CampaignEvent>(), null));
      _aiClientMock.Setup(c => c.GenerateRetrievalQueryEmbeddingAsync(It.IsAny<string>(), It.IsAny<List<CampaignEvent>>(), default)).ReturnsAsync(new float[] { 1.0f });

      await Assert.ThrowsAsync<NotFoundException<Character>>(() => _gameplayService.ProcessTurnAsync(command));
    }

    public static IEnumerable<object[]> EndSessionAsync_ShouldEndSession_WhenCampaignAndActiveSessionExists_Data() {
      yield return new object[] { new EndSessionCommand(Guid.NewGuid(), Guid.NewGuid()) };
    }

    [Theory]
    [MemberData(nameof(EndSessionAsync_ShouldEndSession_WhenCampaignAndActiveSessionExists_Data))]
    public async Task EndSessionAsync_ShouldEndSession_WhenCampaignAndActiveSessionExists(EndSessionCommand command) {
      Session activeSession = new Session { Status = SessionStatus.Active };
      Campaign campaign = new Campaign { Id = command.CampaignId, UserId = command.UserId, Sessions = new List<Session> { activeSession } };
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync(campaign);
      _aiClientMock.Setup(c => c.SummarizeSessionAsync(activeSession, default)).ReturnsAsync("Session Summary");
      _aiClientMock.Setup(c => c.EmbedTextAsync("Session Summary", default)).ReturnsAsync(new float[] { 1.0f });

      await _gameplayService.EndSessionAsync(command);

      Assert.Equal("Session Summary", activeSession.Summary);
      Assert.NotNull(activeSession.SummaryEmbedding);
      Assert.Equal(SessionStatus.Completed, activeSession.Status);
    }

    public static IEnumerable<object[]> EndSessionAsync_ShouldThrowNotFoundException_WhenCampaignNotFound_Data() {
      yield return new object[] { new EndSessionCommand(Guid.NewGuid(), Guid.NewGuid()) };
    }

    [Theory]
    [MemberData(nameof(EndSessionAsync_ShouldThrowNotFoundException_WhenCampaignNotFound_Data))]
    public async Task EndSessionAsync_ShouldThrowNotFoundException_WhenCampaignNotFound(EndSessionCommand command) {
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync((Campaign)null);

      await Assert.ThrowsAsync<NotFoundException<Campaign>>(() => _gameplayService.EndSessionAsync(command));
    }

    public static IEnumerable<object[]> EndSessionAsync_ShouldThrowNotFoundException_WhenNoActiveSession_Data() {
      yield return new object[] { new EndSessionCommand(Guid.NewGuid(), Guid.NewGuid()) };
    }

    [Theory]
    [MemberData(nameof(EndSessionAsync_ShouldThrowNotFoundException_WhenNoActiveSession_Data))]
    public async Task EndSessionAsync_ShouldThrowNotFoundException_WhenNoActiveSession(EndSessionCommand command) {
      Campaign campaign = new Campaign { Id = command.CampaignId, UserId = command.UserId, Sessions = new List<Session>() };
      _campaignRepositoryMock.Setup(r => r.GetByIdAsync(command.CampaignId, command.UserId, default)).ReturnsAsync(campaign);

      await Assert.ThrowsAsync<NotFoundException<Session>>(() => _gameplayService.EndSessionAsync(command));
    }
  }
}
