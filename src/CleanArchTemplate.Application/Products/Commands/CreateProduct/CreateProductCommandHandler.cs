using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.ValueObjects;
using MediatR;

namespace CleanArchTemplate.Application.Products.Commands.CreateProduct;

/// <summary>Cria um produto. O commit é feito pelo <c>UnitOfWorkBehavior</c>, não aqui.</summary>
public sealed class CreateProductCommandHandler(IProductRepository products)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            Sku.Create(command.Sku),
            command.Name,
            Money.Create(command.Price, command.Currency));

        await products.AddAsync(product, cancellationToken);
        return product.Id.Value;
    }
}
