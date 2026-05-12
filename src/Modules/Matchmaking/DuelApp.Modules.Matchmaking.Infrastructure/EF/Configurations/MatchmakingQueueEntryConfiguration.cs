using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;
using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuelApp.Modules.Matchmaking.Infrastructure.EF.Configurations;

internal sealed class MatchmakingQueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.ToTable("queue_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PlayerId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<MatchmakingStatus>(v))
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired(false);

        builder.HasIndex(x => new { x.PlayerId, x.Status });

        builder.HasIndex(x => x.StartedAt);
        
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}