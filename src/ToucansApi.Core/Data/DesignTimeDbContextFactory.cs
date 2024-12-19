using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ToucansApi.Core.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ToucansDbContext>
{
    public ToucansDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ToucansDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ToucansDbContext(optionsBuilder.Options);
    }
}