using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.Configuration;
using ToucansApi.Core.Data;
using ToucansApi.Functions.Interfaces.Repositories;
using ToucansApi.Functions.Middleware;
using ToucansApi.Functions.Repositories;

namespace ToucansApi.Functions;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("local.settings.json", true, true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                services.Configure<Settings>(configuration.GetSection("Settings"));
                services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));

                services.AddDbContext<ToucansDbContext>(options =>
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection")
                                           ?? throw new InvalidOperationException(
                                               "Connection string 'DefaultConnection' not found.");

                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            5,
                            TimeSpan.FromSeconds(30),
                            null);
                    });
                });

                services.AddEventStore(configuration);

                services.AddScoped<ITodoListRepository, TodoListRepository>();
                services.AddScoped<ITodoItemRepository, TodoItemRepository>();
                services.AddScoped<IUserRepository, UserRepository>();

                services.AddApplicationInsightsTelemetryWorkerService();
                services.AddLogging(builder => { builder.AddConsole(); });
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ToucansDbContext>();
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database");
                throw;
            }
        }

        await host.RunAsync();
    }
}