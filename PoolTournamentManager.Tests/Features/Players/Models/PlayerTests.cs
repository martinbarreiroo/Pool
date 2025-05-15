using System;
using System.Collections.Generic;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Players.DTOs;
using Xunit;

namespace PoolTournamentManager.Tests.Features.Players.Models
{
    public class PlayerTests
    {
        [Fact]
        public void Player_Constructor_InitializesCollections()
        {
            // Arrange & Act
            var player = new Player();

            // Assert
            Assert.NotNull(player.MatchesAsPlayer1);
            Assert.NotNull(player.MatchesAsPlayer2);
            Assert.Empty(player.MatchesAsPlayer1);
            Assert.Empty(player.MatchesAsPlayer2);
        }

        [Fact]
        public void Player_Constructor_GeneratesNewGuid()
        {
            // Arrange & Act
            var player = new Player();

            // Assert
            Assert.NotEqual(Guid.Empty, player.Id);
        }

        [Fact]
        public void Player_DefaultProperties_HaveCorrectValues()
        {
            // Arrange & Act
            var player = new Player();

            // Assert
            Assert.Equal(string.Empty, player.Name);
            Assert.Equal(string.Empty, player.Email);
            Assert.Equal(string.Empty, player.ProfilePictureUrl);
            Assert.Null(player.PreferredCue);
            Assert.Equal(0, player.Ranking);
        }

        [Fact]
        public void Player_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var player = new Player
            {
                Name = "Test Player",
                Email = "test@example.com",
                ProfilePictureUrl = "https://example.com/profile.jpg",
                PreferredCue = "Test Cue",
                Ranking = 100
            };

            // Act & Assert
            Assert.Equal("Test Player", player.Name);
            Assert.Equal("test@example.com", player.Email);
            Assert.Equal("https://example.com/profile.jpg", player.ProfilePictureUrl);
            Assert.Equal("Test Cue", player.PreferredCue);
            Assert.Equal(100, player.Ranking);
        }
    }
}