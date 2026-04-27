using System.Runtime.CompilerServices;
using DuelApp.Modules.Matchmaking.Domain.Matchmaking.Entities;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("DuelApp.Migrator")]
namespace DuelApp.Modules.Matchmaking.Infrastructure;

public class MatchmakingDbContext : DbContext
{
    public MatchmakingDbContext(DbContextOptions<MatchmakingDbContext> options)
        : base(options)
    {
    }

    public DbSet<MatchmakingQueueEntry> MatchmakingQueueEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("MatchmakingQueueEntries");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}