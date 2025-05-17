using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoolTournamentManager.Features.Players.Models;

namespace PoolTournamentManager.Shared.Infrastructure.Data.EntityConfigurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.HasKey(p => p.Id);

            // Explicitly mapping Guid to the appropriate DB provider type
            builder.Property(p => p.Id)
                .HasColumnType("uniqueidentifier"); // Uses uniqueidentifier in SQL Server

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Email)
                .HasMaxLength(100);

            builder.Property(p => p.ProfilePictureUrl)
                .IsRequired();

            builder.Property(p => p.PreferredCue)
                .HasMaxLength(100);

            // Configure relationships from the Player side
            builder.HasMany(p => p.MatchesAsPlayer1)
                .WithOne(m => m.Player1)
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.MatchesAsPlayer2)
                .WithOne(m => m.Player2)
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}