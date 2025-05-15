using System;
using System.Collections.Generic;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Players.DTOs;
using Xunit;

namespace PoolTournamentManager.Tests;

/// <summary>
/// Example xUnit test class showing basic test patterns
/// </summary>
public class ExampleTests
{
    /// <summary>
    /// Simple test with Assert.True
    /// </summary>
    [Fact]
    public void SimplePassingTest()
    {
        // Arrange - setup test data and conditions
        bool condition = true;

        // Act - perform the action being tested
        // (No action needed for this simple example)

        // Assert - verify the result
        Assert.True(condition);
    }

    /// <summary>
    /// Test showing the importance of handling equality correctly
    /// </summary>
    [Fact]
    public void StringEqualityTest()
    {
        // Arrange
        string expected = "Hello World";
        string actual = "Hello" + " " + "World";

        // Act - no explicit action needed

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Example of a Theory with InlineData for parameterized tests
    /// </summary>
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(5, 3, 8)]
    [InlineData(-1, 1, 0)]
    public void AdditionTest(int a, int b, int expected)
    {
        // Act
        int result = a + b;

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Simple test with Player model
    /// </summary>
    [Fact]
    public void Player_DefaultConstructor_InitializesCollections()
    {
        // Arrange & Act
        var player = new Player();

        // Assert
        Assert.NotNull(player.MatchesAsPlayer1);
        Assert.NotNull(player.MatchesAsPlayer2);
        Assert.Empty(player.MatchesAsPlayer1);
        Assert.Empty(player.MatchesAsPlayer2);
    }

    /// <summary>
    /// Test that PlayerDto correctly holds values
    /// </summary>
    [Fact]
    public void PlayerDto_Properties_SetAndGetCorrectly()
    {
        // Arrange
        var dto = new PlayerDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Player",
            Email = "test@example.com",
            ProfilePictureUrl = "https://example.com/picture.jpg",
            PreferredCue = "Test Cue",
            Ranking = 100,
            MatchCount = 5
        };

        // Act & Assert
        Assert.Equal("Test Player", dto.Name);
        Assert.Equal("test@example.com", dto.Email);
        Assert.Equal("https://example.com/picture.jpg", dto.ProfilePictureUrl);
        Assert.Equal("Test Cue", dto.PreferredCue);
        Assert.Equal(100, dto.Ranking);
        Assert.Equal(5, dto.MatchCount);
    }
}