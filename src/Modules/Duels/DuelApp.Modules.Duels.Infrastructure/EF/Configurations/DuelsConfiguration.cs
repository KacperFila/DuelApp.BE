using DuelApp.Modules.Duels.Domain.Duels.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuelApp.Modules.Duels.Infrastructure.EF.Configurations;

internal sealed class DuelConfiguration : IEntityTypeConfiguration<Duel>
{
    public void Configure(EntityTypeBuilder<Duel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(255);

        builder.Property(x => x.PlayerOneId)
            .IsRequired();

        builder.Property(x => x.PlayerTwoId)
            .IsRequired();

        builder.Property(x => x.CurrentRound)
            .IsRequired();

        builder.Property(x => x.TotalRounds)
            .IsRequired();

        builder.Property(x => x.PlayerOneScore)
            .IsRequired();

        builder.Property(x => x.PlayerTwoScore)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.OwnsMany(x => x.Rounds, rounds =>
        {
            rounds.ToTable("DuelRounds");

            rounds.WithOwner().HasForeignKey("DuelId");

            rounds.HasKey("DuelId", "Number");

            rounds.Property(x => x.Number)
                .IsRequired();

            rounds.Property(x => x.QuestionId);

            rounds.Property(x => x.HasPlayerOneAnsweredCorrectly);

            rounds.Property(x => x.HasPlayerTwoAnsweredCorrectly);
        });
        
        builder.HasIndex(x => new { x.PlayerOneId, x.Status });

        builder.HasIndex(x => new { x.PlayerTwoId, x.Status });
    }
}