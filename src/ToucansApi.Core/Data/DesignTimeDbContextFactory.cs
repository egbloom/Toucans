using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ToucansApi.Core.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ToucansDbContext>
{
    public ToucansDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ToucansDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=toucans;Username=postgres;Password=your_password");

        return new ToucansDbContext(optionsBuilder.Options);
    }
}