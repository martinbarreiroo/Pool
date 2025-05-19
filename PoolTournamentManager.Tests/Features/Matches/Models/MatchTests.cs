using System;
using PoolTournamentManager.Features.Matches.Models;
using Xunit;

namespace PoolTournamentManager.Tests.Features.Matches.Models
{
    public class MatchTests
    {
        [Fact]
        public void Match_Constructor_GeneratesNewGuid()
        {
            // Arrange & Act
            var match = new Match();

            // Assert
            Assert.NotEqual(Guid.Empty, match.Id);
        }

        [Fact]
        public void Match_DefaultProperties_HaveCorrectValues()
        {
            // Arrange & Act
            var match = new Match();

            // Assert
            Assert.Equal(default, match.ScheduledTime);
            Assert.Null(match.EndTime);
            Assert.Null(match.WinnerId);
            Assert.Null(match.TournamentId);
            Assert.Equal(default(Guid), match.Player1Id);
            Assert.Equal(default(Guid), match.Player2Id);
            Assert.Null(match.Location);
            Assert.Null(match.Tournament);
            Assert.Null(match.Player1);
            Assert.Null(match.Player2);
        }

        [Fact]
        public void Match_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var now = DateTime.Now;
            var player1Id = Guid.NewGuid();
            var player2Id = Guid.NewGuid();
            var winnerId = player1Id;
            var tournamentId = Guid.NewGuid();

            var match = new Match
            {
                ScheduledTime = now,
                EndTime = now.AddHours(1),
                WinnerId = winnerId,
                TournamentId = tournamentId,
                Player1Id = player1Id,
                Player2Id = player2Id,
                Location = "Pool Hall A",
            };

            // Act & Assert
            Assert.Equal(now, match.ScheduledTime);
            Assert.Equal(now.AddHours(1), match.EndTime);
            Assert.Equal(winnerId, match.WinnerId);
            Assert.Equal(tournamentId, match.TournamentId);
            Assert.Equal(player1Id, match.Player1Id);
            Assert.Equal(player2Id, match.Player2Id);
            Assert.Equal("Pool Hall A", match.Location);
        }
    }
}