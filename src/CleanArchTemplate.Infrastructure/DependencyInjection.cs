using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Infrastructure.Persistence;
using CleanArchTemplate.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchTemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var provider = Enum.Parse<DatabaseProvider>(
            configuration["Database:Provider"] ?? nameof(DatabaseProvider.Postgres),
            ignoreCase: true);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default ausente na configuração.");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider == DatabaseProvider.SqlServer)
            {
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sql.MigrationsHistoryTable("__ef_migrations_sqlserver");
                });
            }
            else
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.MigrationsHistoryTable("__ef_migrations_postgres");
                });
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
