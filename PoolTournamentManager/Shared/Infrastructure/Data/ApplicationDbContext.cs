using Microsoft.EntityFrameworkCore;
using PoolTournamentManager.Features.Players.Models;
using PoolTournamentManager.Features.Matches.Models;
using PoolTournamentManager.Features.Tournaments.Models;
using Microsoft.Extensions.Hosting;

namespace PoolTournamentManager.Shared.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string? _migrationAssembly;
        private readonly IHostEnvironment? _environment;

        // Add constructor with IHostEnvironment
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHostEnvironment? environment = null,
            string? migrationAssembly = null)
            : base(options)
        {
            _migrationAssembly = migrationAssembly;
            _environment = environment;
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

            // Apply entity configurations with environment context
            // We need to manually apply configurations to pass the environment
            modelBuilder.ApplyConfiguration(new Shared.Infrastructure.Data.EntityConfigurations.PlayerConfiguration(_environment));
            modelBuilder.ApplyConfiguration(new Shared.Infrastructure.Data.EntityConfigurations.MatchConfiguration());
            modelBuilder.ApplyConfiguration(new Shared.Infrastructure.Data.EntityConfigurations.TournamentConfiguration());

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
