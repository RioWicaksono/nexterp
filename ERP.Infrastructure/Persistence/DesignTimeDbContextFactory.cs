using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ERP.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Allows dotnet-ef tools to create migrations without running the app.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ERPDbContext>
{
    public ERPDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.Length > 0
            ? args[0]
            : "Host=localhost;Port=5432;Database=erp_db;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ERPDbContext>();

        if (connectionString.Contains("sqlite", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(connectionString);
        }
        else
        {
            optionsBuilder.UseNpgsql(connectionString);
        }

        return new ERPDbContext(optionsBuilder.Options, tenantContext: null);
    }
}
