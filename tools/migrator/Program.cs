using System;
using System.Threading.Tasks;
using DuelApp.Modules.Duels.Infrastructure;
using DuelApp.Modules.Matchmaking.Infrastructure;
using DuelApp.Modules.Users.Core.DAL;
using Microsoft.EntityFrameworkCore;

namespace DuelApp.Migrator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        string connectionString = null;
        
        for (int i = 0; i < args.Length; i++)
        {
            if ((args[i] == "--connection" || args[i] == "-c") && i + 1 < args.Length)
            {
                connectionString = args[i + 1];
                break;
            }
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: You must provide a connection string using --connection or -c");
            Console.WriteLine("Example: \n DuelApp.Migrator.exe --connection \"Host=localhost;Port=5555;Database=dueldb;Username=dueluser;Password=duelpass;Pooling=true;TrustServerCertificate=True;\"");
            Console.ForegroundColor = ConsoleColor.Gray;
            return 1;
        }

        try
        {
            var usersOptionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
            var duelsOptionsBuilder = new DbContextOptionsBuilder<DuelsDbContext>();
            var matchmakingOptionsBuilder = new DbContextOptionsBuilder<MatchmakingDbContext>();
            
            usersOptionsBuilder.UseNpgsql(connectionString);
            duelsOptionsBuilder.UseNpgsql(connectionString);
            matchmakingOptionsBuilder.UseNpgsql(connectionString);

            using var usersDbContext = new UsersDbContext(usersOptionsBuilder.Options);
            using var duelsDbContext = new DuelsDbContext(duelsOptionsBuilder.Options);
            using var matchmakingDbContext = new MatchmakingDbContext(matchmakingOptionsBuilder.Options);

            Console.WriteLine("Running migrations...");
            await usersDbContext.Database.MigrateAsync();
            await duelsDbContext.Database.MigrateAsync();
            await matchmakingDbContext.Database.MigrateAsync();
            Console.WriteLine("Migrations completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Migration failed:");
            Console.WriteLine(ex.Message);
            return 1;
        }
    }
}