using System.Reflection;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Infrastructure.Persistence;

/// <summary>DbContext único da aplicação. Provider definido em runtime (ver DependencyInjection).</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
