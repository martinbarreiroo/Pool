using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoolTournamentManager.Features.Tournaments.Models;

namespace PoolTournamentManager.Shared.Infrastructure.Data.EntityConfigurations
{
    public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
    {
        public void Configure(EntityTypeBuilder<Tournament> builder)
        {
            builder.HasKey(t => t.Id);

            // Explicitly mapping Guid to the appropriate DB provider type
            builder.Property(t => t.Id)
                .HasColumnType("uniqueidentifier");

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Location)
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .IsRequired(false);

            builder.Property(t => t.EndDate)
                .IsRequired(false);
        }
    }
}