using CleanArchTemplate.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Common.Abstractions;

/// <summary>Contrato de leitura/escrita do banco exposto à Application (read models das queries).</summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
