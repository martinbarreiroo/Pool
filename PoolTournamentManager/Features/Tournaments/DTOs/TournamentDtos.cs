using System.ComponentModel.DataAnnotations;

namespace PoolTournamentManager.Features.Tournaments.DTOs
{
    public class TournamentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int MatchCount { get; set; }
    }

    public class CreateTournamentDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public string? Description { get; set; }
    }

    public class UpdateTournamentDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}
