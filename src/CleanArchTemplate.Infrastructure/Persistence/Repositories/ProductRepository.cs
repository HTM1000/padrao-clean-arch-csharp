using CleanArchTemplate.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await context.Products.AddAsync(product, cancellationToken);

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default) =>
        await context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
}
