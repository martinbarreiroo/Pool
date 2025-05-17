using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoolTournamentManager.Features.Matches.Models;

namespace PoolTournamentManager.Shared.Infrastructure.Data.EntityConfigurations
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.HasKey(m => m.Id);

            // Explicitly mapping Guid to the appropriate DB provider type
            builder.Property(m => m.Id)
                .HasColumnType("uniqueidentifier");

            builder.Property(m => m.Player1Id)
                .HasColumnType("uniqueidentifier");

            builder.Property(m => m.Player2Id)
                .HasColumnType("uniqueidentifier");

            builder.Property(m => m.TournamentId)
                .HasColumnType("uniqueidentifier");

            // For DateTime properties, we need to properly handle nullability
            // Remove the IsRequired(false) call since it's causing issues
            // builder.Property(m => m.ScheduledTime)
            //    .IsRequired(false);

            // Relationship with Tournament is already defined in ApplicationDbContext
        }
    }
}