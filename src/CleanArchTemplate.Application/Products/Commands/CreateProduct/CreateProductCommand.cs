using CleanArchTemplate.Application.Common.Messaging;

namespace CleanArchTemplate.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Sku, string Name, decimal Price, string Currency)
    : ICommand<Guid>;
