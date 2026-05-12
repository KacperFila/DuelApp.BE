using System.Runtime.CompilerServices;
using DuelApp.Modules.Questions.Domain.Questions.Entities;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("DuelApp.Migrator")]
namespace DuelApp.Modules.Questions.Infrastructure;

public class QuestionsDbContext : DbContext
{
    public QuestionsDbContext(DbContextOptions<QuestionsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Questions");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}