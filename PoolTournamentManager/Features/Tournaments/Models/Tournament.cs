using System.ComponentModel.DataAnnotations;

namespace PoolTournamentManager.Features.Tournaments.Models
{
    public class Tournament
    {
        public Tournament()
        {
            Matches = new HashSet<Matches.Models.Match>();
        }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        // Navigation property
        public virtual ICollection<Matches.Models.Match> Matches { get; set; }
    }
}
