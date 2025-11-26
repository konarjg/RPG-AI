
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application;
using Application.Dtos;
using Application.Dtos.Commands;
using Application.Factories;
using Application.Interfaces;
using Application.Ports.Data;
using Application.Ports.Llm;
using Application.Ports.Repositories;
using Domain.Entities;
using Moq;
using Xunit;

namespace Tests.Application
{
    public class GameSystemServiceTests
    {
        // Test Data Constants
        private const string TestTitle = "Test Game System";
        private const string TestOverview = "This is a test game system.";
        private static readonly Guid TestUserId = Guid.NewGuid();
        private static readonly Guid TestSystemId = Guid.NewGuid();
        private static readonly Dictionary<string, object> TestSchema = new() { { "character", "schema" } };
        private static readonly string[] TestChunks = { "chunk1", "chunk2" };
        private static readonly List<EmbeddingResult> TestEmbeddings = new()
        {
            new EmbeddingResult(new float[] { 1.0f, 2.0f }, 10),
            new EmbeddingResult(new float[] { 3.0f, 4.0f }, 20)
        };

        // Mocks
        private readonly Mock<ISchemaLoader> _schemaLoaderMock;
        private readonly Mock<IEmbeddingCalculator> _embeddingCalculatorMock;
        private readonly Mock<IRulebookSimplifier> _rulebookSimplifierMock;
        private readonly Mock<IGameRuleChunkRepository> _gameRuleChunkRepositoryMock;
        private readonly Mock<IGameSystemRepository> _gameSystemRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly GameSystemService _sut;

        public GameSystemServiceTests()
        {
            _schemaLoaderMock = new Mock<ISchemaLoader>();
            _embeddingCalculatorMock = new Mock<IEmbeddingCalculator>();
            _rulebookSimplifierMock = new Mock<IRulebookSimplifier>();
            _gameRuleChunkRepositoryMock = new Mock<IGameRuleChunkRepository>();
            _gameSystemRepositoryMock = new Mock<IGameSystemRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _sut = new GameSystemService(
                _schemaLoaderMock.Object,
                _embeddingCalculatorMock.Object,
                _rulebookSimplifierMock.Object,
                _gameRuleChunkRepositoryMock.Object,
                _gameSystemRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task UploadSystemAsync_ShouldUploadSystem_WhenCommandIsValid()
        {
            // Arrange
            var command = new UploadGameSystemCommand(
                TestTitle,
                TestOverview,
                new MemoryStream(),
                new MemoryStream()
            );

            _schemaLoaderMock.Setup(x => x.LoadSchemaAsync(It.IsAny<Stream>()))
                .ReturnsAsync(TestSchema);
            _rulebookSimplifierMock.Setup(x => x.SimplifyRulebookAsync(It.IsAny<Stream>()))
                .ReturnsAsync(TestChunks);
            _embeddingCalculatorMock.Setup(x => x.EmbedAsync(TestChunks))
                .ReturnsAsync(TestEmbeddings);

            // Act
            var result = await _sut.UploadSystemAsync(command, TestUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestTitle, result.Title);
            Assert.Equal(TestOverview, result.Overview);
            Assert.Equal(TestSchema, result.CharacterSheetSchema);
            Assert.Equal(TestUserId, result.UserId);

            _gameSystemRepositoryMock.Verify(x => x.Add(It.Is<GameSystem>(gs =>
                gs.Title == TestTitle &&
                gs.Overview == TestOverview &&
                gs.CharacterSheetSchema == TestSchema &&
                gs.UserId == TestUserId
            )), Times.Once);

            _gameRuleChunkRepositoryMock.Verify(x => x.AddRange(It.Is<List<GameRuleChunk>>(chunks =>
                chunks.Count == TestEmbeddings.Count
            )), Times.Once);

            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        public static IEnumerable<object[]> GetSystemByIdAsyncData()
        {
            yield return new object[] { TestSystemId, TestUserId, new GameSystem(TestSystemId, TestTitle, TestOverview, TestSchema, TestUserId) };
            yield return new object[] { Guid.NewGuid(), TestUserId, null };
        }

        [Theory]
        [MemberData(nameof(GetSystemByIdAsyncData))]
        public async Task GetSystemByIdAsync_ShouldReturnSystem_WhenSystemExists(Guid systemId, Guid? userId, GameSystem expectedSystem)
        {
            // Arrange
            _gameSystemRepositoryMock.Setup(x => x.GetByIdAsync(systemId, userId))
                .ReturnsAsync(expectedSystem);

            // Act
            var result = await _sut.GetSystemByIdAsync(systemId, userId);

            // Assert
            Assert.Equal(expectedSystem, result);
        }

        public static IEnumerable<object[]> GetSystemBySearchQueryAsyncData()
        {
            yield return new object[] { "test", TestUserId, new GameSystem(TestSystemId, TestTitle, TestOverview, TestSchema, TestUserId) };
            yield return new object[] { "nonexistent", TestUserId, null };
        }

        [Theory]
        [MemberData(nameof(GetSystemBySearchQueryAsyncData))]
        public async Task GetSystemBySearchQueryAsync_ShouldReturnSystem_WhenSystemMatchesQuery(string query, Guid? userId, GameSystem expectedSystem)
        {
            // Arrange
            _gameSystemRepositoryMock.Setup(x => x.GetBySearchQueryAsync(query, userId))
                .ReturnsAsync(expectedSystem);

            // Act
            var result = await _sut.GetSystemBySearchQueryAsync(query, userId);

            // Assert
            Assert.Equal(expectedSystem, result);
        }

        public static IEnumerable<object[]> BrowseSystemsAsyncData()
        {
            var systems = new List<GameSystem>
            {
                new GameSystem(Guid.NewGuid(), "System 1", "Overview 1", new Dictionary<string, object>(), TestUserId),
                new GameSystem(Guid.NewGuid(), "System 2", "Overview 2", new Dictionary<string, object>(), TestUserId)
            };
            yield return new object[] { TestUserId, null, 10, new PagedResult<GameSystem>(systems, null) };
            yield return new object[] { TestUserId, Guid.NewGuid(), 5, new PagedResult<GameSystem>(new List<GameSystem>(), null) };
        }

        [Theory]
        [MemberData(nameof(BrowseSystemsAsyncData))]
        public async Task BrowseSystemsAsync_ShouldReturnPagedSystems(Guid? userId, Guid? pointer, int pageSize, PagedResult<GameSystem> expectedResult)
        {
            // Arrange
            _gameSystemRepositoryMock.Setup(x => x.BrowseAsync(userId, pointer, pageSize))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.BrowseSystemsAsync(userId, pointer, pageSize);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
