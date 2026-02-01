using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using DuelApp.Modules.Users.Core.Entities;

[assembly: InternalsVisibleTo("DuelApp.Migrator")]
namespace DuelApp.Modules.Users.Core.DAL
{
    internal class UsersDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("users");
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }
    }
}
