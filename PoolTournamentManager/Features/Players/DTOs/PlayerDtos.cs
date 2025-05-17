using System.ComponentModel.DataAnnotations;

namespace PoolTournamentManager.Features.Players.DTOs
{
    /// <summary>
    /// Data transfer object for player information
    /// </summary>
    public class PlayerDto
    {
        /// <summary>
        /// Unique identifier for the player
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid Id { get; set; }

        /// <summary>
        /// Player's full name
        /// </summary>
        /// <example>John Smith</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Player's email address
        /// </summary>
        /// <example>john.smith@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// URL to the player's profile picture
        /// </summary>
        /// <example>https://example.com/images/players/profile123.jpg</example>
        public string ProfilePictureUrl { get; set; } = string.Empty;

        /// <summary>
        /// Player's preferred cue brand/model
        /// </summary>
        /// <example>Predator Revo</example>
        public string? PreferredCue { get; set; }

        /// <summary>
        /// Player's current ranking
        /// </summary>
        /// <example>1200</example>
        public int Ranking { get; set; }

        /// <summary>
        /// Total number of matches played by the player
        /// </summary>
        /// <example>42</example>
        public int MatchCount { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a new player
    /// </summary>
    public class CreatePlayerDto
    {
        /// <summary>
        /// Player's full name (required)
        /// </summary>
        /// <example>Jane Doe</example>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Player's email address
        /// </summary>
        /// <example>jane.doe@example.com</example>
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Player's preferred cue brand/model
        /// </summary>
        /// <example>McDermott G-Core</example>
        [StringLength(100)]
        public string? PreferredCue { get; set; }

        /// <summary>
        /// Content type of the profile picture to be uploaded
        /// </summary>
        /// <example>image/jpeg</example>
        public string ContentType { get; set; } = "image/jpeg";
    }

    /// <summary>
    /// Response object returned after creating a new player
    /// </summary>
    public class CreatePlayerResponseDto
    {
        /// <summary>
        /// The created player information
        /// </summary>
        public PlayerDto Player { get; set; } = new PlayerDto();

        /// <summary>
        /// Pre-signed URL for uploading the player's profile picture
        /// </summary>
        /// <example>https://bucket.s3.amazonaws.com/uploads/abc123?X-Amz-Algorithm=...</example>
        public string PresignedUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for updating an existing player
    /// </summary>
    public class UpdatePlayerDto
    {
        /// <summary>
        /// Updated player name (optional)
        /// </summary>
        /// <example>Jane Smith</example>
        [StringLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// Updated email address (optional)
        /// </summary>
        /// <example>jane.smith@example.com</example>
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Updated profile picture URL (optional)
        /// </summary>
        /// <example>https://example.com/images/players/updated-profile.jpg</example>
        public string? ProfilePictureUrl { get; set; }

        /// <summary>
        /// Updated preferred cue (optional)
        /// </summary>
        /// <example>Viking Valhalla</example>
        [StringLength(100)]
        public string? PreferredCue { get; set; }
    }

    /// <summary>
    /// Response object returned after generating a profile picture upload URL
    /// </summary>
    public class UploadProfilePictureResponseDto
    {
        /// <summary>
        /// Pre-signed URL for uploading the player's profile picture
        /// </summary>
        /// <example>https://bucket.s3.amazonaws.com/uploads/abc123?X-Amz-Algorithm=...</example>
        public string PresignedUrl { get; set; } = string.Empty;

        /// <summary>
        /// The URL where the profile picture will be accessible after upload
        /// </summary>
        /// <example>https://bucket.s3.amazonaws.com/players/123/profile.jpg</example>
        public string ProfilePictureUrl { get; set; } = string.Empty;
    }
}
