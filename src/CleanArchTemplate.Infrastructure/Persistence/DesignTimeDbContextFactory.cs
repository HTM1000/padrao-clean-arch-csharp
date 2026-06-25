using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanArchTemplate.Infrastructure.Persistence;

/// <summary>
/// Usado pelo <c>dotnet ef</c> em design-time. Provider via <c>-- --provider Postgres|SqlServer</c>.
/// A connection string vem da env var <c>ConnectionStrings__Default</c>; o fallback local é só para
/// scaffolding de migrations (nunca usado em runtime, que lê de appsettings/secret manager).
/// </summary>
[SuppressMessage(
    "Security",
    "S2068:Hard-coded credentials are security-sensitive",
    Justification = "Fallback de design-time apenas para gerar migrations localmente; runtime usa configuração.")]
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var provider = ParseProvider(args);
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var assemblyName = typeof(AppDbContext).Assembly.FullName;
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");

        if (provider == DatabaseProvider.SqlServer)
        {
            builder.UseSqlServer(
                connectionString ?? "Server=localhost;Database=cleanarch_design;Trusted_Connection=True;TrustServerCertificate=True",
                options => options.MigrationsAssembly(assemblyName));
        }
        else
        {
            builder.UseNpgsql(
                connectionString ?? "Host=localhost;Database=cleanarch_design;Username=postgres;Password=postgres",
                options => options.MigrationsAssembly(assemblyName));
        }

        return new AppDbContext(builder.Options);
    }

    private static DatabaseProvider ParseProvider(string[] args)
    {
        var index = Array.IndexOf(args, "--provider");
        var isSqlServer = index >= 0
            && index + 1 < args.Length
            && string.Equals(args[index + 1], "SqlServer", StringComparison.OrdinalIgnoreCase);

        return isSqlServer ? DatabaseProvider.SqlServer : DatabaseProvider.Postgres;
    }
}
