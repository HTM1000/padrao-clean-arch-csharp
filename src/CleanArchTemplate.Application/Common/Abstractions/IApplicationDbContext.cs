using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Common.Abstractions;

/// <summary>Contrato de leitura/escrita do banco exposto à Application (read models das queries).</summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }

    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
