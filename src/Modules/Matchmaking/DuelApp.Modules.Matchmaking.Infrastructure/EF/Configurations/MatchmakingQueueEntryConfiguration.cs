using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DuelApp.Modules.Matchmaking.Infrastructure.EF.Configurations;

internal sealed class MatchmakingQueueEntryConfiguration : IEntityTypeConfiguration<MatchmakingQueueEntry>
{
    public void Configure(EntityTypeBuilder<MatchmakingQueueEntry> builder)
    {
        builder.ToTable("matchmaking_queue_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PlayerId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired(false);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.StartedAt);

        builder.HasIndex(x => new { x.Status, x.StartedAt });
        
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
        
        builder.HasIndex(x => x.PlayerId)
            .IsUnique();
    }
}