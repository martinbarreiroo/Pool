using Microsoft.EntityFrameworkCore;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Matches.Models;
using PoolTournamentManager.Features.Tournaments.Models;

namespace PoolTournamentManager.Shared.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string? _migrationAssembly;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, string? migrationAssembly = null)
            : base(options)
        {
            _migrationAssembly = migrationAssembly;
        }

        // This constructor is used by EF Core tools
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Player entity relationships with matches
            modelBuilder.Entity<Player>()
                .HasMany(p => p.MatchesAsPlayer1)
                .WithOne(m => m.Player1)
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Player>()
                .HasMany(p => p.MatchesAsPlayer2)
                .WithOne(m => m.Player2)
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Match entity relationship with tournament
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
