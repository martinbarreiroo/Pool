using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using PoolTournamentManager.Features.Players.DTOs;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Players.Services;
using PoolTournamentManager.Shared.Infrastructure.Data;
using PoolTournamentManager.Tests.Mocks;

namespace PoolTournamentManager.Tests.Features.Players.Services
{
    public class PlayerServiceTests : IDisposable
    {
        // Helper method to create mock configuration sections
        private IConfigurationSection CreateConfigSection(string value)
        {
            var section = Substitute.For<IConfigurationSection>();
            section.Value.Returns(value);
            return section;
        }

        private readonly ApplicationDbContext _dbContext;
        private readonly PlayerService _playerService;
        private readonly MockStorageService _mockStorageService;

        public PlayerServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Create a mock storage service that implements IStorageService
            _mockStorageService = new MockStorageService(
                defaultPresignedUrl: "https://test-presigned-url.com",
                defaultObjectUrl: "https://test-bucket.s3.amazonaws.com/test-image.jpg"
            );

            // Use the actual PlayerService with our mock storage service
            _playerService = new PlayerService(_dbContext, _mockStorageService);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task CreatePlayerAsync_WithValidData_ReturnsSuccessfully()
        {
            // Arrange
            var createDto = new CreatePlayerDto
            {
                Name = "John Doe",
                Email = "john@example.com",
                PreferredCue = "Custom Cue",
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _playerService.CreatePlayerAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Name, result.Player.Name);
            Assert.Equal(createDto.Email, result.Player.Email);
            Assert.Equal(createDto.PreferredCue, result.Player.PreferredCue);

            // Verify player was added to database
            var playerInDb = await _dbContext.Players.FindAsync(result.Player.Id);
            Assert.NotNull(playerInDb);
        }

        [Fact]
        public async Task GetPlayerByIdAsync_WithExistingId_ReturnsPlayer()
        {
            // Arrange
            var player = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Jane Smith",
                Email = "jane@example.com",
                ProfilePictureUrl = "https://example.com/profile.jpg",
                PreferredCue = "House Cue",
                Ranking = 5
            };

            await _dbContext.Players.AddAsync(player);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _playerService.GetPlayerByIdAsync(player.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(player.Id, result.Id);
            Assert.Equal(player.Name, result.Name);
            Assert.Equal(player.Email, result.Email);
            Assert.Equal(player.ProfilePictureUrl, result.ProfilePictureUrl);
            Assert.Equal(player.PreferredCue, result.PreferredCue);
            Assert.Equal(player.Ranking, result.Ranking);
        }

        [Fact]
        public async Task GetPlayerByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _playerService.GetPlayerByIdAsync(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllPlayersAsync_ReturnsAllPlayers()
        {
            // Arrange
            await _dbContext.Players.AddRangeAsync(new List<Player>
            {
                new Player { Id = Guid.NewGuid(), Name = "Player 1", Email = "player1@example.com" },
                new Player { Id = Guid.NewGuid(), Name = "Player 2", Email = "player2@example.com" },
                new Player { Id = Guid.NewGuid(), Name = "Player 3", Email = "player3@example.com" }
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _playerService.GetAllPlayersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, p => p.Name == "Player 1");
            Assert.Contains(result, p => p.Name == "Player 2");
            Assert.Contains(result, p => p.Name == "Player 3");
        }

        [Fact]
        public async Task CreatePlayerAsync_WithInvalidData_ThrowsException()
        {
            // Arrange
            var createDto = new CreatePlayerDto
            {
                Name = "", // Invalid empty name
                Email = "invalid-email",
                ContentType = "invalid/type"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _playerService.CreatePlayerAsync(createDto));
        }

        [Fact]
        public async Task GetAllPlayersAsync_WithSearchTerm_ReturnsMatchingPlayers()
        {
            // Arrange
            await _dbContext.Players.AddRangeAsync(new List<Player>
            {
                new Player { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com" },
                new Player { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com" },
                new Player { Id = Guid.NewGuid(), Name = "John Smith", Email = "johnsmith@example.com" }
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _playerService.GetAllPlayersAsync("John");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Name == "John Doe");
            Assert.Contains(result, p => p.Name == "John Smith");
            Assert.DoesNotContain(result, p => p.Name == "Jane Smith");
        }

        [Fact]
        public async Task CreatePlayerAsync_Always_SetsProfilePictureUrl()
        {
            // Arrange
            var createDto = new CreatePlayerDto
            {
                Name = "Test Player",
                Email = "test@example.com",
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _playerService.CreatePlayerAsync(createDto);

            // Assert
            Assert.NotNull(result.Player.ProfilePictureUrl);
            Assert.NotEmpty(result.Player.ProfilePictureUrl);
        }

        [Fact]
        public async Task CreatePlayerAsync_VerifiesProfilePictureUrlIsFromMock()
        {
            // Arrange
            var createDto = new CreatePlayerDto
            {
                Name = "Test Player",
                Email = "test@example.com",
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _playerService.CreatePlayerAsync(createDto);

            // Assert
            // Verify that the profile picture URL came from our mock
            Assert.Equal("https://test-bucket.s3.amazonaws.com/test-image.jpg", result.Player.ProfilePictureUrl);
            Assert.Equal("https://test-presigned-url.com", result.PresignedUrl);
        }

        [Fact]
        public async Task CreatePlayerAsync_WithInvalidContentType_ThrowsException()
        {
            // Arrange
            var createDto = new CreatePlayerDto
            {
                Name = "John Doe",
                Email = "john@example.com",
                PreferredCue = "Custom Cue",
                ContentType = "application/pdf" // Invalid content type
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _playerService.CreatePlayerAsync(createDto));

            // Verify the exception contains the expected message
            Assert.Contains("Content type application/pdf is not allowed", exception.Message);
            Assert.Contains("Allowed types: image/jpeg, image/png", exception.Message);
        }

        [Fact]
        public async Task StorageService_CheckAccess_ReturnsConfiguredValue()
        {
            // Arrange
            var mockStorageService = new MockStorageService(isStorageAccessible: true);
            var playerService = new PlayerService(_dbContext, mockStorageService);

            // Act
            bool isAccessible = await mockStorageService.CheckAccessAsync();

            // Assert
            Assert.True(isAccessible);

            // Arrange - With inaccessible storage
            mockStorageService = new MockStorageService(isStorageAccessible: false);
            playerService = new PlayerService(_dbContext, mockStorageService);

            // Act
            isAccessible = await mockStorageService.CheckAccessAsync();

            // Assert
            Assert.False(isAccessible);
        }
    }
}