using System.ComponentModel.DataAnnotations;

namespace PoolTournamentManager.Features.Players.Models
{
    public class Player
    {
        public Player()
        {
            MatchesAsPlayer1 = new HashSet<Matches.Models.Match>();
            MatchesAsPlayer2 = new HashSet<Matches.Models.Match>();
        }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ProfilePictureUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string? PreferredCue { get; set; }

        public int Ranking { get; set; } = 0;

        public virtual ICollection<Matches.Models.Match> MatchesAsPlayer1 { get; set; }

        public virtual ICollection<Matches.Models.Match> MatchesAsPlayer2 { get; set; }
    }
}
