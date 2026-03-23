using System.Runtime.CompilerServices;
using DuelApp.Modules.Duels.Domain.Duels.Entities;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("DuelApp.Migrator")]
namespace DuelApp.Modules.Duels.Infrastructure;

public class DuelsDbContext : DbContext
{
    public DuelsDbContext(DbContextOptions<DuelsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Duel> Duels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Duels");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}