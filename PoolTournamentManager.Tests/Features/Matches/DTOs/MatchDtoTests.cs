using System;
using PoolTournamentManager.Features.Matches.DTOs;
using Xunit;

namespace PoolTournamentManager.Tests.Features.Matches.DTOs
{
    public class MatchDtoTests
    {
        [Fact]
        public void MatchDto_DefaultConstructor_InitializesProperties()
        {
            // Arrange & Act
            var dto = new MatchDto();

            // Assert
            Assert.Equal(Guid.Empty, dto.Id);
            Assert.Equal(default, dto.ScheduledTime);
            Assert.Null(dto.EndTime);
            Assert.Null(dto.WinnerId);
            Assert.Null(dto.TournamentId);
            Assert.Null(dto.TournamentName);
            Assert.Equal(Guid.Empty, dto.Player1Id);
            Assert.Equal(Guid.Empty, dto.Player2Id);
            Assert.Null(dto.Player1);
            Assert.Null(dto.Player2);
            Assert.Null(dto.Location);
            Assert.Null(dto.Notes);
            Assert.Null(dto.Player1Score);
            Assert.Null(dto.Player2Score);
        }

        [Fact]
        public void MatchDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            var player1Id = Guid.NewGuid();
            var player2Id = Guid.NewGuid();
            var tournamentId = Guid.NewGuid();
            var player1 = new PlayerSummaryDto { Id = player1Id, Name = "Player 1" };
            var player2 = new PlayerSummaryDto { Id = player2Id, Name = "Player 2" };

            var dto = new MatchDto
            {
                Id = id,
                ScheduledTime = now,
                EndTime = now.AddHours(1),
                WinnerId = player1Id,
                TournamentId = tournamentId,
                TournamentName = "Tournament 1",
                Player1Id = player1Id,
                Player2Id = player2Id,
                Player1 = player1,
                Player2 = player2,
                Location = "Pool Hall A",
                Notes = "Championship match",
                Player1Score = 5,
                Player2Score = 3
            };

            // Act & Assert
            Assert.Equal(id, dto.Id);
            Assert.Equal(now, dto.ScheduledTime);
            Assert.Equal(now.AddHours(1), dto.EndTime);
            Assert.Equal(player1Id, dto.WinnerId);
            Assert.Equal(tournamentId, dto.TournamentId);
            Assert.Equal("Tournament 1", dto.TournamentName);
            Assert.Equal(player1Id, dto.Player1Id);
            Assert.Equal(player2Id, dto.Player2Id);
            Assert.Same(player1, dto.Player1);
            Assert.Same(player2, dto.Player2);
            Assert.Equal("Pool Hall A", dto.Location);
            Assert.Equal("Championship match", dto.Notes);
            Assert.Equal(5, dto.Player1Score);
            Assert.Equal(3, dto.Player2Score);
        }

        [Fact]
        public void CreateMatchDto_DefaultConstructor_InitializesProperties()
        {
            // Arrange & Act
            var dto = new CreateMatchDto();

            // Assert
            Assert.Equal(default, dto.ScheduledTime);
            Assert.Equal(Guid.Empty, dto.Player1Id);
            Assert.Equal(Guid.Empty, dto.Player2Id);
            Assert.Null(dto.TournamentId);
            Assert.Null(dto.Location);
            Assert.Null(dto.Notes);
        }

        [Fact]
        public void UpdateMatchDto_DefaultConstructor_InitializesProperties()
        {
            // Arrange & Act
            var dto = new UpdateMatchDto();

            // Assert
            Assert.Null(dto.ScheduledTime);
            Assert.Null(dto.EndTime);
            Assert.Null(dto.WinnerId);
            Assert.Null(dto.TournamentId);
            Assert.Null(dto.Player1Id);
            Assert.Null(dto.Player2Id);
            Assert.Null(dto.Location);
            Assert.Null(dto.Notes);
            Assert.Null(dto.Player1Score);
            Assert.Null(dto.Player2Score);
        }

        [Fact]
        public void PlayerSummaryDto_DefaultConstructor_InitializesProperties()
        {
            // Arrange & Act
            var dto = new PlayerSummaryDto();

            // Assert
            Assert.Equal(Guid.Empty, dto.Id);
            Assert.Equal(string.Empty, dto.Name);
            Assert.Equal(string.Empty, dto.ProfilePictureUrl);
        }

        [Fact]
        public void PlayerSummaryDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new PlayerSummaryDto
            {
                Id = id,
                Name = "Test Player",
                ProfilePictureUrl = "https://example.com/profile.jpg"
            };

            // Act & Assert
            Assert.Equal(id, dto.Id);
            Assert.Equal("Test Player", dto.Name);
            Assert.Equal("https://example.com/profile.jpg", dto.ProfilePictureUrl);
        }
    }
}