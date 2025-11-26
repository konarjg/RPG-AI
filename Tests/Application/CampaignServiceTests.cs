using Application;
using Application.Dtos;
using Application.Exceptions;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using Moq;

namespace Tests.Application;

public class CampaignServiceTests
{
  private readonly Mock<ICampaignRepository> _campaignRepositoryMock;
  private readonly Mock<IUserRepository> _userRepositoryMock;
  private readonly Mock<IGameSystemRepository> _gameSystemRepositoryMock;
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly CampaignService _campaignService;

  public CampaignServiceTests()
  {
    _campaignRepositoryMock = new Mock<ICampaignRepository>();
    _userRepositoryMock = new Mock<IUserRepository>();
    _gameSystemRepositoryMock = new Mock<IGameSystemRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _campaignService = new CampaignService(
      _campaignRepositoryMock.Object,
      _userRepositoryMock.Object,
      _gameSystemRepositoryMock.Object,
      _unitOfWorkMock.Object);
  }

  private static readonly Guid GameMasterUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
  private static readonly Guid GameSystemId = Guid.Parse("00000000-0000-0000-0000-000000000002");
  private static readonly Guid CampaignId = Guid.Parse("00000000-0000-0000-0000-000000000003");

  public static IEnumerable<object[]> CreateCampaignAsync_ValidData_CreatesCampaign_Data =>
    new List<object[]>
    {
      new object[] { new CreateCampaignCommand(GameMasterUserId, GameSystemId, "Test Campaign", "Test Overview") }
    };

  [Theory]
  [MemberData(nameof(CreateCampaignAsync_ValidData_CreatesCampaign_Data))]
  public async Task CreateCampaignAsync_ValidData_CreatesCampaign(CreateCampaignCommand command)
  {
    _userRepositoryMock.Setup(r => r.GetByIdAsync(command.GameMasterUserId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new User { Id = command.GameMasterUserId });
    _gameSystemRepositoryMock.Setup(r => r.GetByIdAsync(command.GameSystemId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new GameSystem { Id = command.GameSystemId });

    Campaign result = await _campaignService.CreateCampaignAsync(command);

    Assert.NotNull(result);
    Assert.Equal(command.Name, result.Name);
    Assert.Equal(command.Overview, result.Overview);
    Assert.Equal(command.GameMasterUserId, result.GameMasterUserId);
    Assert.Equal(command.GameSystemId, result.GameSystemId);
    _campaignRepositoryMock.Verify(r => r.Add(It.IsAny<Campaign>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  public static IEnumerable<object[]> CreateCampaignAsync_InvalidData_ThrowsArgumentException_Data =>
    new List<object[]>
    {
      new object[] { new CreateCampaignCommand(GameMasterUserId, GameSystemId, "", "Test Overview") }
    };

  [Theory]
  [MemberData(nameof(CreateCampaignAsync_InvalidData_ThrowsArgumentException_Data))]
  public async Task CreateCampaignAsync_InvalidData_ThrowsArgumentException(CreateCampaignCommand command)
  {
    _userRepositoryMock.Setup(r => r.GetByIdAsync(command.GameMasterUserId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new User { Id = command.GameMasterUserId });
    _gameSystemRepositoryMock.Setup(r => r.GetByIdAsync(command.GameSystemId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new GameSystem { Id = command.GameSystemId });

    await Assert.ThrowsAsync<ArgumentException>(() => _campaignService.CreateCampaignAsync(command));
  }

  public static IEnumerable<object[]> CreateCampaignAsync_GameMasterNotFound_ThrowsNotFoundException_Data =>
    new List<object[]>
    {
      new object[] { new CreateCampaignCommand(GameMasterUserId, GameSystemId, "Test Campaign", "Test Overview") }
    };

  [Theory]
  [MemberData(nameof(CreateCampaignAsync_GameMasterNotFound_ThrowsNotFoundException_Data))]
  public async Task CreateCampaignAsync_GameMasterNotFound_ThrowsNotFoundException(CreateCampaignCommand command)
  {
    _userRepositoryMock.Setup(r => r.GetByIdAsync(command.GameMasterUserId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((User)null);

    await Assert.ThrowsAsync<NotFoundException<User>>(() => _campaignService.CreateCampaignAsync(command));
  }

  public static IEnumerable<object[]> CreateCampaignAsync_GameSystemNotFound_ThrowsNotFoundException_Data =>
    new List<object[]>
    {
      new object[] { new CreateCampaignCommand(GameMasterUserId, GameSystemId, "Test Campaign", "Test Overview") }
    };

  [Theory]
  [MemberData(nameof(CreateCampaignAsync_GameSystemNotFound_ThrowsNotFoundException_Data))]
  public async Task CreateCampaignAsync_GameSystemNotFound_ThrowsNotFoundException(CreateCampaignCommand command)
  {
    _userRepositoryMock.Setup(r => r.GetByIdAsync(command.GameMasterUserId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new User { Id = command.GameMasterUserId });
    _gameSystemRepositoryMock.Setup(r => r.GetByIdAsync(command.GameSystemId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((GameSystem)null);

    await Assert.ThrowsAsync<NotFoundException<GameSystem>>(() => _campaignService.CreateCampaignAsync(command));
  }

  public static IEnumerable<object[]> GetCampaignStateAsync_CampaignExists_ReturnsCampaign_Data =>
    new List<object[]>
    {
      new object[] { CampaignId, GameMasterUserId }
    };

  [Theory]
  [MemberData(nameof(GetCampaignStateAsync_CampaignExists_ReturnsCampaign_Data))]
  public async Task GetCampaignStateAsync_CampaignExists_ReturnsCampaign(Guid campaignId, Guid userId)
  {
    Campaign campaign = new Campaign { Id = campaignId, GameMasterUserId = userId, GameSystemId = GameSystemId, Name = "Test Campaign", Overview = "Test Overview" };
    _campaignRepositoryMock.Setup(r => r.GetByIdAsync(campaignId, userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(campaign);

    Campaign result = await _campaignService.GetCampaignStateAsync(campaignId, userId);

    Assert.NotNull(result);
    Assert.Equal(campaignId, result.Id);
    Assert.Equal(userId, result.GameMasterUserId);
    Assert.Equal("Test Campaign", result.Name);
    Assert.Equal("Test Overview", result.Overview);
  }

  public static IEnumerable<object[]> GetCampaignStateAsync_UserIsNotGameMaster_ReturnsNull_Data =>
    new List<object[]>
    {
      new object[] { CampaignId, Guid.NewGuid() }
    };

  [Theory]
  [MemberData(nameof(GetCampaignStateAsync_UserIsNotGameMaster_ReturnsNull_Data))]
  public async Task GetCampaignStateAsync_UserIsNotGameMaster_ReturnsNull(Guid campaignId, Guid userId)
  {
    _campaignRepositoryMock.Setup(r => r.GetByIdAsync(campaignId, userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Campaign)null);

    Campaign result = await _campaignService.GetCampaignStateAsync(campaignId, userId);

    Assert.Null(result);
  }

  public static IEnumerable<object[]> GetCampaignStateAsync_CampaignNotFound_ReturnsNull_Data =>
    new List<object[]>
    {
      new object[] { CampaignId, GameMasterUserId }
    };

  [Theory]
  [MemberData(nameof(GetCampaignStateAsync_CampaignNotFound_ReturnsNull_Data))]
  public async Task GetCampaignStateAsync_CampaignNotFound_ReturnsNull(Guid campaignId, Guid userId)
  {
    _campaignRepositoryMock.Setup(r => r.GetByIdAsync(campaignId, userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Campaign)null);

    Campaign result = await _campaignService.GetCampaignStateAsync(campaignId, userId);

    Assert.Null(result);
  }

  public static IEnumerable<object[]> GetAllCampaignsAsync_CampaignsExist_ReturnsCampaigns_Data =>
    new List<object[]>
    {
      new object[] { GameMasterUserId }
    };

  [Theory]
  [MemberData(nameof(GetAllCampaignsAsync_CampaignsExist_ReturnsCampaigns_Data))]
  public async Task GetAllCampaignsAsync_CampaignsExist_ReturnsCampaigns(Guid userId)
  {
    List<Campaign> campaigns = new List<Campaign>
    {
      new Campaign { Id = Guid.NewGuid(), GameMasterUserId = userId, GameSystemId = GameSystemId, Name = "Test Campaign 1", Overview = "Test Overview 1" },
      new Campaign { Id = Guid.NewGuid(), GameMasterUserId = userId, GameSystemId = GameSystemId, Name = "Test Campaign 2", Overview = "Test Overview 2" }
    };
    _campaignRepositoryMock.Setup(r => r.GetAllAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(campaigns);

    List<Campaign> result = await _campaignService.GetAllCampaignsAsync(userId);

    Assert.NotNull(result);
    Assert.Equal(2, result.Count);
  }

  public static IEnumerable<object[]> GetAllCampaignsAsync_NoCampaignsExist_ReturnsEmptyList_Data =>
    new List<object[]>
    {
      new object[] { GameMasterUserId }
    };

  [Theory]
  [MemberData(nameof(GetAllCampaignsAsync_NoCampaignsExist_ReturnsEmptyList_Data))]
  public async Task GetAllCampaignsAsync_NoCampaignsExist_ReturnsEmptyList(Guid userId)
  {
    _campaignRepositoryMock.Setup(r => r.GetAllAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Campaign>());

    List<Campaign> result = await _campaignService.GetAllCampaignsAsync(userId);

    Assert.NotNull(result);
    Assert.Empty(result);
  }
}
