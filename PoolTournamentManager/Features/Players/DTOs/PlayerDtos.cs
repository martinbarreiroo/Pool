using System.ComponentModel.DataAnnotations;

namespace PoolTournamentManager.Features.Players.DTOs
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string? PreferredCue { get; set; }
        public int Ranking { get; set; }
        public int MatchCount { get; set; }
    }

    public class CreatePlayerDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? PreferredCue { get; set; }

        public string ContentType { get; set; } = "image/jpeg";
    }

    public class CreatePlayerResponseDto
    {
        public PlayerDto Player { get; set; } = new PlayerDto();
        public string PresignedUrl { get; set; } = string.Empty;
    }

    public class UpdatePlayerDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public string? ProfilePictureUrl { get; set; }

        [StringLength(100)]
        public string? PreferredCue { get; set; }
    }

    public class UploadProfilePictureResponseDto
    {
        public string PresignedUrl { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
    }
}
