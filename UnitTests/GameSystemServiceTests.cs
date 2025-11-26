using Application;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Application.Dtos;
using Domain.Ports.Infrastructure.Dtos;
using FluentAssertions;
using NSubstitute;

namespace UnitTests;

public class GameSystemServiceTests
{
  private readonly IAiClient _aiClient;
  private readonly ISchemaProvider _schemaProvider;
  private readonly IGameSystemRepository _gameSystemRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly GameSystemService _sut;

  public GameSystemServiceTests()
  {
    _aiClient = Substitute.for<IAiClient>();
    _schemaProvider = Substitute.for<ISchemaProvider>();
    _gameSystemRepository = Substitute.for<IGameSystemRepository>();
    _unitOfWork = Substitute.for<IUnitOfWork>();
    _sut = new GameSystemService(_aiClient, _schemaProvider, _gameSystemRepository, _unitOfWork);
  }

  [Fact]
  public async Task GetSystemByIdAsync_ShouldReturnGameSystem_WhenGameSystemExists()
  {
    GameSystem gameSystem = new()
    {
      Id = Guid.NewGuid(),
      Title = "Dungeons & Dragons",
      Overview = "Fantasy role-playing game.",
      CharacterSheetSchema = new Dictionary<string, object>(),
      Rules = new List<GameRuleChunk>(),
      CharacterCreationGuide = "Follow the steps in the Player's Handbook."
    };
    _gameSystemRepository.GetByIdAsync(gameSystem.Id).Returns(gameSystem);

    GameSystem? result = await _sut.GetSystemByIdAsync(gameSystem.Id);

    result.Should().Be(gameSystem);
  }

  [Fact]
  public async Task GetSystemByIdAsync_ShouldReturnNull_WhenGameSystemDoesNotExist()
  {
    Guid id = Guid.NewGuid();
    _gameSystemRepository.GetByIdAsync(id).Returns((GameSystem?)null);

    GameSystem? result = await _sut.GetSystemByIdAsync(id);

    result.Should().BeNull();
  }

  [Theory]
  [InlineData(null, 10)]
  [InlineData("cursor", 5)]
  public async Task BrowseSystemsAsync_ShouldReturnCursorResult_WhenCalled(string? cursor, int pageSize)
  {
    CursorResult<GameSystem> expectedResult = new CursorResult<GameSystem>(new List<GameSystem>(), "nextCursor");
    _gameSystemRepository.GetAllAsync(cursor, pageSize).Returns(expectedResult);

    CursorResult<GameSystem> result = await _sut.BrowseSystemsAsync(cursor, pageSize);

    result.Should().Be(expectedResult);
  }

  [Fact]
  public async Task UploadSystemAsync_ShouldCreateAndReturnGameSystem_WhenCommandIsValid()
  {
    GameSystem expectedGameSystem = new()
    {
      Title = "Dungeons & Dragons",
      Overview = "Fantasy role-playing game.",
      CharacterSheetSchema = new Dictionary<string, object>(),
    };

    UploadGameSystemCommand command = new(
      expectedGameSystem.Title,
      expectedGameSystem.Overview,
      new MemoryStream(),
      new MemoryStream()
    );

    _schemaProvider.LoadSchemaAsync(command.CharacterSheetSchemaStream, default).Returns(expectedGameSystem.CharacterSheetSchema);

    AiRulebookChunkResponse chunkResponse = new(new[]
    {
      new RulebookChunk("Rule 1", RulebookChunkTag.General),
      new RulebookChunk("Rule 2", RulebookChunkTag.CharacterCreation)
    });
    _aiClient.ChunkRulebookAsync(command.RulebookStream, default).Returns(chunkResponse);

    float[] embedding1 = { 1.0f };
    float[] embedding2 = { 2.0f };
    _aiClient.EmbedTextAsync("Rule 1", default).Returns(embedding1);
    _aiClient.EmbedTextAsync("Rule 2", default).Returns(embedding2);

    GameSystem? addedGameSystem = null;
    _gameSystemRepository.Add(Arg.Do<GameSystem>(gs => addedGameSystem = gs));

    GameSystem result = await _sut.UploadSystemAsync(command);

    await _unitOfWork.Received(1).SaveChangesAsync();
    result.Should().Be(addedGameSystem);
    result.Title.Should().Be(expectedGameSystem.Title);
    result.Overview.Should().Be(expectedGameSystem.Overview);
    result.CharacterSheetSchema.Should().BeEquivalentTo(expectedGameSystem.CharacterSheetSchema);
    result.CharacterCreationGuide.Should().Be("Rule 2");
    result.Rules.Should().HaveCount(2);
    result.Rules.ElementAt(0).Content.Should().Be("Rule 1");
    result.Rules.ElementAt(0).ContentEmbedding.Should().BeEquivalentTo(embedding1);
    result.Rules.ElementAt(1).Content.Should().Be("Rule 2");
    result.Rules.ElementAt(1).ContentEmbedding.Should().BeEquivalentTo(embedding2);
  }

  [Fact]
  public async Task UploadSystemAsync_ShouldHandleEmptyRulebookStream()
  {
    GameSystem expectedGameSystem = new()
    {
      Title = "Dungeons & Dragons",
      Overview = "Fantasy role-playing game.",
      CharacterSheetSchema = new Dictionary<string, object>(),
    };

    UploadGameSystemCommand command = new(
      expectedGameSystem.Title,
      expectedGameSystem.Overview,
      new MemoryStream(),
      new MemoryStream()
    );

    _schemaProvider.LoadSchemaAsync(command.CharacterSheetSchemaStream, default).Returns(expectedGameSystem.CharacterSheetSchema);

    AiRulebookChunkResponse chunkResponse = new(Array.Empty<RulebookChunk>());
    _aiClient.ChunkRulebookAsync(command.RulebookStream, default).Returns(chunkResponse);

    GameSystem? addedGameSystem = null;
    _gameSystemRepository.Add(Arg.Do<GameSystem>(gs => addedGameSystem = gs));

    GameSystem result = await _sut.UploadSystemAsync(command);

    await _unitOfWork.Received(1).SaveChangesAsync();
    result.Should().Be(addedGameSystem);
    result.Rules.Should().BeEmpty();
    result.CharacterCreationGuide.Should().BeEmpty();
  }

  [Fact]
  public async Task UploadSystemAsync_ShouldHandleNoCharacterCreationGuide()
  {
    GameSystem expectedGameSystem = new()
    {
      Title = "Dungeons & Dragons",
      Overview = "Fantasy role-playing game.",
      CharacterSheetSchema = new Dictionary<string, object>(),
    };

    UploadGameSystemCommand command = new(
      expectedGameSystem.Title,
      expectedGameSystem.Overview,
      new MemoryStream(),
      new MemoryStream()
    );

    _schemaProvider.LoadSchemaAsync(command.CharacterSheetSchemaStream, default).Returns(expectedGameSystem.CharacterSheetSchema);

    AiRulebookChunkResponse chunkResponse = new(new[]
    {
      new RulebookChunk("Rule 1", RulebookChunkTag.General)
    });
    _aiClient.ChunkRulebookAsync(command.RulebookStream, default).Returns(chunkResponse);
    _aiClient.EmbedTextAsync("Rule 1", default).Returns(new float[] { 1.0f });

    GameSystem? addedGameSystem = null;
    _gameSystemRepository.Add(Arg.Do<GameSystem>(gs => addedGameSystem = gs));

    GameSystem result = await _sut.UploadSystemAsync(command);

    await _unitOfWork.Received(1).SaveChangesAsync();
    result.Should().Be(addedGameSystem);
    result.CharacterCreationGuide.Should().BeEmpty();
  }
}
