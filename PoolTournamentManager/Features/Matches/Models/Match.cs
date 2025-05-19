using System.ComponentModel.DataAnnotations;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Tournaments.Models;

namespace PoolTournamentManager.Features.Matches.Models
{
    public class Match
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime ScheduledTime { get; set; }

        public DateTime? EndTime { get; set; }

        public Guid? WinnerId { get; set; }

        public Guid? TournamentId { get; set; }

        [Required]
        public Guid Player1Id { get; set; }

        [Required]
        public Guid Player2Id { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        // Navigation properties 
        public virtual Tournament? Tournament { get; set; }
        public virtual Player? Player1 { get; set; }
        public virtual Player? Player2 { get; set; }
    }
}
