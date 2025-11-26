using Application;
using Application.Dtos;
using Application.Exceptions;
using Domain.Entities;
using Domain.Ports.Infrastructure;
using FluentAssertions;
using NSubstitute;

namespace UnitTests;

public class CampaignServiceTests {
  private readonly ICampaignRepository _campaignRepository = Substitute.For<ICampaignRepository>();
  private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
  private readonly IGameSystemRepository _gameSystemRepository = Substitute.For<IGameSystemRepository>();
  private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
  private readonly CampaignService _sut;

  public CampaignServiceTests() {
    _sut = new CampaignService(_campaignRepository, _userRepository, _gameSystemRepository, _unitOfWork);
  }

  public static IEnumerable<object[]> GetCampaignStateAsync_TestData() {
    Campaign campaign = new() {
      Id = Guid.NewGuid(),
      Name = "Test Campaign",
      Overview = "Test Overview",
      GameMasterUserId = Guid.NewGuid(),
      GameSystemId = Guid.NewGuid()
    };
    yield return new object[] { campaign, campaign };
    yield return new object[] { null, null };
  }

  [Theory]
  [MemberData(nameof(GetCampaignStateAsync_TestData))]
  public async Task GetCampaignStateAsync_ShouldReturnCampaignOrNull(Campaign? campaign, Campaign? expectedResult) {
    Guid id = campaign?.Id ?? Guid.NewGuid();
    Guid userId = campaign?.GameMasterUserId ?? Guid.NewGuid();
    _campaignRepository.GetByIdAsync(id, userId).Returns(campaign);

    Campaign? result = await _sut.GetCampaignStateAsync(id, userId);

    result.Should().Be(expectedResult);
  }

  [Fact]
  public async Task GetAllCampaignsAsync_ShouldReturnListOfCampaigns() {
    Guid userId = Guid.NewGuid();
    List<Campaign> campaigns = new() {
      new() { Name = "Campaign 1", Overview = "Overview 1", GameMasterUserId = userId, GameSystemId = Guid.NewGuid() },
      new() { Name = "Campaign 2", Overview = "Overview 2", GameMasterUserId = userId, GameSystemId = Guid.NewGuid() }
    };
    _campaignRepository.GetAllAsync(userId).Returns(campaigns);

    List<Campaign> result = await _sut.GetAllCampaignsAsync(userId);

    result.Should().BeEquivalentTo(campaigns);
  }

  [Fact]
  public async Task CreateCampaignAsync_ShouldCreateAndReturnCampaign_WhenCommandIsValid() {
    CreateCampaignCommand command = new("Test Campaign", "Test Overview", Guid.NewGuid(), Guid.NewGuid());
    _userRepository.GetByIdAsync(command.GameMasterUserId).Returns(new User());
    _gameSystemRepository.GetByIdAsync(command.GameSystemId).Returns(new GameSystem());
    Campaign? addedCampaign = null;
    _campaignRepository.Add(Arg.Do<Campaign>(c => addedCampaign = c));

    Campaign result = await _sut.CreateCampaignAsync(command);

    await _unitOfWork.Received(1).SaveChangesAsync();
    result.Should().Be(addedCampaign);
    result.Name.Should().Be(command.Name);
    result.Overview.Should().Be(command.Overview);
    result.GameMasterUserId.Should().Be(command.GameMasterUserId);
    result.GameSystemId.Should().Be(command.GameSystemId);
  }

  public static IEnumerable<object[]> CreateCampaignAsync_NotFound_TestData() {
    yield return new object[] { true, false, typeof(NotFoundException<User>) };
    yield return new object[] { false, true, typeof(NotFoundException<GameSystem>) };
  }

  [Theory]
  [MemberData(nameof(CreateCampaignAsync_NotFound_TestData))]
  public async Task CreateCampaignAsync_ShouldThrowNotFoundException_WhenDependencyIsMissing(bool userIsNull, bool gameSystemIsNull, Type exceptionType) {
    CreateCampaignCommand command = new("Test Campaign", "Test Overview", Guid.NewGuid(), Guid.NewGuid());
    _userRepository.GetByIdAsync(command.GameMasterUserId).Returns(userIsNull ? null : new User());
    _gameSystemRepository.GetByIdAsync(command.GameSystemId).Returns(gameSystemIsNull ? null : new GameSystem());

    await _sut.Awaiting(s => s.CreateCampaignAsync(command))
        .Should().ThrowAsync<Exception>().Where(e => e.GetType() == exceptionType);
  }
}
