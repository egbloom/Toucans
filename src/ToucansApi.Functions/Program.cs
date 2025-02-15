using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
            .ConfigureFunctionsWebApplication()
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.UseMiddleware<ExceptionHandlingMiddleware>();
                builder.UseWhen<HealthCheckMiddleware>(context =>
                    context.FunctionDefinition.EntryPoint.Contains("HealthCheckFunction"));
            })
            .ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", true,
                        true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                                       ?? throw new InvalidOperationException(
                                           "Connection string 'DefaultConnection' not found.");
                services.Configure<Settings>(configuration.GetSection("Settings"));
                services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));

                services.AddDbContext<ToucansDbContext>(options =>
                {
                    var dbConnectionString = configuration.GetConnectionString("DefaultConnection")
                                             ?? throw new InvalidOperationException(
                                                 "Connection string 'DefaultConnection' not found.");

                    options.UseNpgsql(dbConnectionString, npgsqlOptions =>
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

                services.AddHealthChecks()
                    .AddDbContextCheck<ToucansDbContext>()
                    .AddNpgSql(
                        connectionString,
                        name: "postgresql",
                        tags: ["db", "postgresql"]);
                services.AddScoped<HealthCheckService>();
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