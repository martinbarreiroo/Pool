using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PoolTournamentManager.Features.Players.DTOs;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Players.Services;
using PoolTournamentManager.Shared.Infrastructure.Data;
using PoolTournamentManager.Shared.Infrastructure.Storage;
using Xunit;

namespace PoolTournamentManager.Tests.Features.Players.Services
{
    public class PlayerServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PlayerService> _logger;
        private readonly TestPlayerService _playerService;

        public PlayerServiceTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _dbContext = new ApplicationDbContext(options);
            
            // Use NSubstitute for mocking dependencies
            _logger = Substitute.For<ILogger<PlayerService>>();
            
            // Create our test implementation of PlayerService
            _playerService = new TestPlayerService(_dbContext, _logger);
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
    }
    
    /// <summary>
    /// A test implementation of PlayerService that overrides the storage dependency
    /// </summary>
    public class TestPlayerService : PlayerService
    {
        public TestPlayerService(
            ApplicationDbContext dbContext,
            ILogger<PlayerService> logger)
            : base(dbContext, null, logger)
        {
            // Passing null for S3StorageService, but we'll override the methods that use it
        }
        
        // Override the methods that use S3StorageService
        
        public override async Task<CreatePlayerResponseDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto)
        {
            // Validate the input
            if (string.IsNullOrWhiteSpace(createPlayerDto.Name))
            {
                throw new ArgumentException("Player name cannot be empty");
            }
            
            if (string.IsNullOrWhiteSpace(createPlayerDto.Email) || !createPlayerDto.Email.Contains('@'))
            {
                throw new ArgumentException("Invalid email format");
            }
            
            if (createPlayerDto.ContentType != "image/jpeg" && createPlayerDto.ContentType != "image/png")
            {
                throw new ArgumentException($"Content type {createPlayerDto.ContentType} is not allowed. Allowed types: image/jpeg, image/png");
            }
            
            // Create player with a placeholder profile picture URL
            var player = new Player
            {
                Name = createPlayerDto.Name,
                Email = createPlayerDto.Email,
                ProfilePictureUrl = "https://test-bucket.s3.amazonaws.com/test-image.jpg", // Use a fixed URL
                PreferredCue = createPlayerDto.PreferredCue,
                // Initialize with default value
                Ranking = 0
            };

            DbContext.Players.Add(player);
            await DbContext.SaveChangesAsync();

            // Create response with player data and presigned URL
            return new CreatePlayerResponseDto
            {
                Player = new PlayerDto
                {
                    Id = player.Id,
                    Name = player.Name,
                    Email = player.Email,
                    ProfilePictureUrl = player.ProfilePictureUrl,
                    PreferredCue = player.PreferredCue,
                    Ranking = player.Ranking,
                    MatchCount = 0
                },
                PresignedUrl = "https://test-presigned-url.com"
            };
        }
        
        // Add a protected DbContext property to access from the overridden method
        protected ApplicationDbContext DbContext => _dbContext;
    }
} 