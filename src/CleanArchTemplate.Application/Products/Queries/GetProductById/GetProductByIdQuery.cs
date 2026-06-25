using CleanArchTemplate.Application.Common.Messaging;

namespace CleanArchTemplate.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
