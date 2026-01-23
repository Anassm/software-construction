using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using v2.Infrastructure.Data;
using v2.Infrastructure.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "v2", "infrastructure", "data", "app.db");
            options.UseSqlite($"Data Source={dbPath}");
        });


        builder.Services.AddScoped<ImportService>();    

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var importer = scope.ServiceProvider.GetRequiredService<ImportService>();

        string basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "v1", "data"));
        string sessionPath = Path.Combine(basePath, "pdata");

        try
        {
            System.Console.WriteLine("Starting data import...\n");
            System.Console.WriteLine("database location: " + Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "v2", "infrastructure", "data", "app.db")));

            Console.WriteLine("Importing Users...");
            await importer.ImportUsersAsync(Path.Combine(basePath, "users.json"));
            Console.WriteLine("Users imported");

            Console.WriteLine("Importing Vehicles...");
            await importer.ImportVehiclesAsync(Path.Combine(basePath, "vehicles.json"));
            Console.WriteLine("Vehicles imported");

            Console.WriteLine("Importing Parking Lots...");
            await importer.ImportParkingLotsAsync(Path.Combine(basePath, "parking-lots.json"));
            Console.WriteLine("Parking Lots imported");

            Console.WriteLine("Importing Reservations...");
            await importer.ImportReservationsAsync(Path.Combine(basePath, "reservations.json"));
            Console.WriteLine("Reservations imported");

            Console.WriteLine("Importing Sessions...");
            await importer.ImportAllSessionsAsync(sessionPath);
            Console.WriteLine("All Sessions imported");

            Console.WriteLine("Importing Payments...");
            await importer.ImportPaymentsAsync(Path.Combine(basePath, "payments.json"));
            Console.WriteLine("Payments imported");

            Console.WriteLine("\nData import completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nERROR during import:");
            Console.WriteLine(ex);
        }
    }
} 
