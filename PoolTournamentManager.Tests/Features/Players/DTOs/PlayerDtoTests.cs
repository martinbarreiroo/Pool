using System;
using PoolTournamentManager.Features.Players.DTOs;
using Xunit;

namespace PoolTournamentManager.Tests.Features.Players.DTOs
{
    public class PlayerDtoTests
    {
        [Fact]
        public void PlayerDto_DefaultConstructor_InitializesProperties()
        {
            // Arrange & Act
            var dto = new PlayerDto();
            
            // Assert
            Assert.Equal(Guid.Empty, dto.Id);
            Assert.Equal(string.Empty, dto.Name);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.ProfilePictureUrl);
            Assert.Null(dto.PreferredCue);
            Assert.Equal(0, dto.Ranking);
            Assert.Equal(0, dto.MatchCount);
        }
        
        [Fact]
        public void PlayerDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new PlayerDto
            {
                Id = id,
                Name = "Test Player",
                Email = "test@example.com",
                ProfilePictureUrl = "https://example.com/profile.jpg",
                PreferredCue = "Test Cue",
                Ranking = 100,
                MatchCount = 10
            };
            
            // Act & Assert
            Assert.Equal(id, dto.Id);
            Assert.Equal("Test Player", dto.Name);
            Assert.Equal("test@example.com", dto.Email);
            Assert.Equal("https://example.com/profile.jpg", dto.ProfilePictureUrl);
            Assert.Equal("Test Cue", dto.PreferredCue);
            Assert.Equal(100, dto.Ranking);
            Assert.Equal(10, dto.MatchCount);
        }
        
        [Fact]
        public void CreatePlayerDto_DefaultConstructor_InitializesProperties()
        {
            // Arrange & Act
            var dto = new CreatePlayerDto();
            
            // Assert
            Assert.Equal(string.Empty, dto.Name);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Null(dto.PreferredCue);
            Assert.Equal("image/jpeg", dto.ContentType);
        }
        
        [Fact]
        public void CreatePlayerResponseDto_DefaultConstructor_InitializesProperties()
        {
            // Arrange & Act
            var dto = new CreatePlayerResponseDto();
            
            // Assert
            Assert.NotNull(dto.Player);
            Assert.Equal(string.Empty, dto.PresignedUrl);
        }
        
        [Fact]
        public void UpdatePlayerDto_AllPropertiesNullByDefault()
        {
            // Arrange & Act
            var dto = new UpdatePlayerDto();
            
            // Assert
            Assert.Null(dto.Name);
            Assert.Null(dto.Email);
            Assert.Null(dto.ProfilePictureUrl);
            Assert.Null(dto.PreferredCue);
        }
        
        [Fact]
        public void UploadProfilePictureResponseDto_DefaultConstructor_InitializesProperties()
        {
            // Arrange & Act
            var dto = new UploadProfilePictureResponseDto();
            
            // Assert
            Assert.Equal(string.Empty, dto.PresignedUrl);
            Assert.Equal(string.Empty, dto.ProfilePictureUrl);
        }
    }
} 