using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Products.Queries.GetProductById;

/// <summary>Lê o read model direto do DbContext, projetando o DTO (sem repositório de agregado).</summary>
public sealed class GetProductByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var productId = new ProductId(query.Id);

        var dto = await context.Products
            .Where(product => product.Id == productId)
            .Select(product => new ProductDto(
                product.Id.Value,
                product.Sku.Value,
                product.Name,
                product.Price.Amount,
                product.Price.Currency))
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Error.NotFound("Product.NotFound", $"Produto {query.Id} não encontrado.")
            : dto;
    }
}
