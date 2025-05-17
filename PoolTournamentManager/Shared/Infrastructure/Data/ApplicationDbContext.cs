using Microsoft.EntityFrameworkCore;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Matches.Models;
using PoolTournamentManager.Features.Tournaments.Models;

namespace PoolTournamentManager.Shared.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string? _migrationAssembly;

        // Single constructor with optional parameter
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, string? migrationAssembly = null)
            : base(options)
        {
            _migrationAssembly = migrationAssembly;
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Suppress the pending model changes warning
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations 
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // These relationships can be removed as they are now defined in entity configurations
            // modelBuilder.Entity<Player>()
            //     .HasMany(p => p.MatchesAsPlayer1)
            //     .WithOne(m => m.Player1)
            //     .HasForeignKey(m => m.Player1Id)
            //     .OnDelete(DeleteBehavior.Restrict);
            // 
            // modelBuilder.Entity<Player>()
            //     .HasMany(p => p.MatchesAsPlayer2)
            //     .WithOne(m => m.Player2)
            //     .HasForeignKey(m => m.Player2Id)
            //     .OnDelete(DeleteBehavior.Restrict);
            // 
            // modelBuilder.Entity<Match>()
            //     .HasOne(m => m.Tournament)
            //     .WithMany(t => t.Matches)
            //     .HasForeignKey(m => m.TournamentId)
            //     .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
