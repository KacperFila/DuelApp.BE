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

    public DbSet<QueueEntry> QueueEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Matchmaking");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
