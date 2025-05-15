using System.ComponentModel.DataAnnotations;

namespace PoolTournamentManager.Features.Matches.DTOs
{
    public class MatchDto
    {
        public Guid Id { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Guid? WinnerId { get; set; }
        public Guid? TournamentId { get; set; }
        public string? TournamentName { get; set; }
        public Guid Player1Id { get; set; }
        public Guid Player2Id { get; set; }
        public PlayerSummaryDto? Player1 { get; set; }
        public PlayerSummaryDto? Player2 { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public int? Player1Score { get; set; }
        public int? Player2Score { get; set; }
    }

    public class CreateMatchDto
    {
        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required]
        public Guid Player1Id { get; set; }

        [Required]
        public Guid Player2Id { get; set; }

        public Guid? TournamentId { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateMatchDto
    {
        public DateTime? ScheduledTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Guid? WinnerId { get; set; }
        public Guid? TournamentId { get; set; }
        public Guid? Player1Id { get; set; }
        public Guid? Player2Id { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public int? Player1Score { get; set; }
        public int? Player2Score { get; set; }
    }

    public class PlayerSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
    }
}
