namespace CleanArchTemplate.Domain.Products;

/// <summary>Coleção persistente de produtos (port; implementado na Infrastructure).</summary>
public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);
}
