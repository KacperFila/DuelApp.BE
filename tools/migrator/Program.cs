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
            var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new UsersDbContext(optionsBuilder.Options);

            Console.WriteLine("Running migrations...");
            await context.Database.MigrateAsync();
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