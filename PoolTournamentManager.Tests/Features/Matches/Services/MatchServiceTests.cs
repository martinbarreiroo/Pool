using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PoolTournamentManager.Features.Matches.DTOs;
using PoolTournamentManager.Features.Matches.Models;
using PoolTournamentManager.Features.Matches.Services;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Shared.Exceptions;
using PoolTournamentManager.Shared.Infrastructure.Data;
using Xunit;

namespace PoolTournamentManager.Tests.Features.Matches.Services
{
    public class MatchServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MatchService> _logger;
        private readonly MatchService _matchService;

        public MatchServiceTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Use NSubstitute for mocking dependencies
            _logger = Substitute.For<ILogger<MatchService>>();

            // Create the match service
            _matchService = new MatchService(_dbContext, _logger);

            // Seed the database with test data
            SeedDatabase();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        private void SeedDatabase()
        {
            // Add players
            var player1 = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Player 1",
                Email = "player1@example.com"
            };

            var player2 = new Player
            {
                Id = Guid.NewGuid(),
                Name = "Player 2",
                Email = "player2@example.com"
            };

            _dbContext.Players.AddRange(player1, player2);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task CreateMatchAsync_WithValidData_ReturnsSuccessfully()
        {
            // Arrange
            var player1 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 1");
            var player2 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 2");

            Assert.NotNull(player1);
            Assert.NotNull(player2);

            var scheduledTime = DateTime.Now.AddDays(1);
            var createDto = new CreateMatchDto
            {
                ScheduledTime = scheduledTime,
                Player1Id = player1!.Id,
                Player2Id = player2!.Id,
                Location = "Pool Hall A"
            };

            // Act
            var result = await _matchService.CreateMatchAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.ScheduledTime, result.ScheduledTime);
            Assert.Equal(createDto.Player1Id, result.Player1Id);
            Assert.Equal(createDto.Player2Id, result.Player2Id);
            Assert.Equal(createDto.Location, result.Location);

            // Verify match was added to database
            var matchInDb = await _dbContext.Matches.FindAsync(result.Id);
            Assert.NotNull(matchInDb);
        }

        [Fact]
        public async Task GetMatchByIdAsync_WithExistingId_ReturnsMatch()
        {
            // Arrange
            var player1 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 1");
            var player2 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 2");

            Assert.NotNull(player1);
            Assert.NotNull(player2);

            var match = new Match
            {
                Id = Guid.NewGuid(),
                ScheduledTime = DateTime.Now.AddDays(1),
                Player1Id = player1!.Id,
                Player2Id = player2!.Id,
                Location = "Pool Hall B",
                Notes = "Test match"
            };

            await _dbContext.Matches.AddAsync(match);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _matchService.GetMatchByIdAsync(match.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(match.Id, result.Id);
            Assert.Equal(match.ScheduledTime, result.ScheduledTime);
            Assert.Equal(match.Player1Id, result.Player1Id);
            Assert.Equal(match.Player2Id, result.Player2Id);
            Assert.Equal(match.Location, result.Location);
            Assert.Equal(match.Notes, result.Notes);
        }

        [Fact]
        public async Task GetMatchByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _matchService.GetMatchByIdAsync(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllMatchesAsync_ReturnsAllMatches()
        {
            // Arrange
            var player1 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 1");
            var player2 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 2");

            Assert.NotNull(player1);
            Assert.NotNull(player2);

            await _dbContext.Matches.AddRangeAsync(new List<Match>
            {
                new Match { Id = Guid.NewGuid(), ScheduledTime = DateTime.Now.AddDays(1), Player1Id = player1!.Id, Player2Id = player2!.Id },
                new Match { Id = Guid.NewGuid(), ScheduledTime = DateTime.Now.AddDays(2), Player1Id = player1.Id, Player2Id = player2.Id },
                new Match { Id = Guid.NewGuid(), ScheduledTime = DateTime.Now.AddDays(3), Player1Id = player1.Id, Player2Id = player2.Id }
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _matchService.GetAllMatchesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task CreateMatchAsync_WithSamePlayer_ThrowsException()
        {
            // Arrange
            var player1 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 1");

            Assert.NotNull(player1);

            var createDto = new CreateMatchDto
            {
                ScheduledTime = DateTime.Now.AddDays(1),
                Player1Id = player1!.Id,
                Player2Id = player1.Id, // Same player
                Location = "Pool Hall A"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _matchService.CreateMatchAsync(createDto));
        }

        [Fact]
        public async Task DeleteMatchAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            var player1 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 1");
            var player2 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 2");

            Assert.NotNull(player1);
            Assert.NotNull(player2);

            var match = new Match
            {
                Id = Guid.NewGuid(),
                ScheduledTime = DateTime.Now.AddDays(1),
                Player1Id = player1!.Id,
                Player2Id = player2!.Id
            };

            await _dbContext.Matches.AddAsync(match);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _matchService.DeleteMatchAsync(match.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await _dbContext.Matches.FindAsync(match.Id));
        }

        [Fact]
        public async Task UpdateMatchAsync_WithValidData_ReturnsUpdatedMatch()
        {
            // Arrange
            var player1 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 1");
            var player2 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 2");

            Assert.NotNull(player1);
            Assert.NotNull(player2);

            var match = new Match
            {
                Id = Guid.NewGuid(),
                ScheduledTime = DateTime.Now.AddDays(1),
                Player1Id = player1!.Id,
                Player2Id = player2!.Id,
                Location = "Original Location"
            };

            await _dbContext.Matches.AddAsync(match);
            await _dbContext.SaveChangesAsync();

            var newScheduledTime = DateTime.Now.AddDays(2);
            var updateDto = new UpdateMatchDto
            {
                ScheduledTime = newScheduledTime,
                Location = "Updated Location",
                Player1Score = 5,
                Player2Score = 3
            };

            // Act
            var result = await _matchService.UpdateMatchAsync(match.Id, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newScheduledTime, result.ScheduledTime);
            Assert.Equal("Updated Location", result.Location);
            Assert.Equal(5, result.Player1Score);
            Assert.Equal(3, result.Player2Score);

            // Verify match was updated in database
            var matchInDb = await _dbContext.Matches.FindAsync(match.Id);
            Assert.NotNull(matchInDb);
            Assert.Equal(newScheduledTime, matchInDb!.ScheduledTime);
            Assert.Equal("Updated Location", matchInDb.Location);
            Assert.Equal(5, matchInDb.Player1Score);
            Assert.Equal(3, matchInDb.Player2Score);
        }

        [Fact]
        public async Task GetAllMatchesAsync_WithFilters_ReturnsFilteredMatches()
        {
            // Arrange
            var player1 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 1");
            var player2 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 2");
            var tournamentId = Guid.NewGuid();

            Assert.NotNull(player1);
            Assert.NotNull(player2);

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var nextWeek = today.AddDays(7);

            await _dbContext.Matches.AddRangeAsync(new List<Match>
            {
                new Match { Id = Guid.NewGuid(), ScheduledTime = tomorrow, Player1Id = player1!.Id, Player2Id = player2!.Id, TournamentId = tournamentId },
                new Match { Id = Guid.NewGuid(), ScheduledTime = nextWeek, Player1Id = player1.Id, Player2Id = player2.Id, TournamentId = tournamentId },
                new Match { Id = Guid.NewGuid(), ScheduledTime = nextWeek, Player1Id = player2.Id, Player2Id = player1.Id }
            });
            await _dbContext.SaveChangesAsync();

            // Act - Filter by tournament
            var tournamentMatches = await _matchService.GetAllMatchesAsync(tournamentId: tournamentId);

            // Assert
            Assert.Equal(2, tournamentMatches.Count);
            Assert.All(tournamentMatches, m => Assert.Equal(tournamentId, m.TournamentId));

            // Act - Filter by date range
            var dateRangeMatches = await _matchService.GetAllMatchesAsync(
                startDate: tomorrow,
                endDate: tomorrow.AddDays(2));

            // Assert
            Assert.Single(dateRangeMatches);
            Assert.Equal(tomorrow, dateRangeMatches.First().ScheduledTime);

            // Act - Filter by player
            var playerMatches = await _matchService.GetAllMatchesAsync(playerId: player1.Id);

            // Assert
            Assert.Equal(3, playerMatches.Count);
        }

        [Fact]
        public async Task CreateMatchAsync_WithConcurrentScheduling_ThrowsScheduleConflictException()
        {
            // Arrange
            var player1 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 1");
            var player2 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 2");
            var player3 = await _dbContext.Players.FirstOrDefaultAsync(p => p.Name == "Player 3")
                ?? new Player { Id = Guid.NewGuid(), Name = "Player 3", Email = "player3@example.com" };

            Assert.NotNull(player1);
            Assert.NotNull(player2);

            // If player3 doesn't exist yet, add it to the database
            if (player3.Id == Guid.Empty)
            {
                _dbContext.Players.Add(player3);
                await _dbContext.SaveChangesAsync();
            }

            // Define a specific scheduled time for conflict testing
            var scheduledTime = DateTime.Now.AddDays(1).Date.AddHours(14); // 2pm tomorrow

            // Create first match
            var createDto1 = new CreateMatchDto
            {
                ScheduledTime = scheduledTime,
                Player1Id = player1!.Id,
                Player2Id = player2!.Id,
                Location = "Pool Hall A"
            };

            // Act - Create the first match (should succeed)
            var result = await _matchService.CreateMatchAsync(createDto1);
            Assert.NotNull(result);

            // Try to schedule a second match with the same player at the same time
            var createDto2 = new CreateMatchDto
            {
                ScheduledTime = scheduledTime,
                Player1Id = player1.Id,
                Player2Id = player3.Id,
                Location = "Pool Hall B"
            };

            // Act & Assert - Second match should cause a conflict
            var exception = await Assert.ThrowsAsync<ScheduleConflictException>(() =>
                _matchService.CreateMatchAsync(createDto2));

            // Verify exception contains correct details
            Assert.Equal(player1.Id, exception.Player1Id);
            Assert.Equal(player3.Id, exception.Player2Id);
            Assert.Equal(scheduledTime, exception.ConflictTime);
        }
    }
}
