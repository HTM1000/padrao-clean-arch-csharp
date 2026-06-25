namespace CleanArchTemplate.Application.Products.Queries.GetProductById;

public sealed record ProductDto(Guid Id, string Sku, string Name, decimal Price, string Currency);
