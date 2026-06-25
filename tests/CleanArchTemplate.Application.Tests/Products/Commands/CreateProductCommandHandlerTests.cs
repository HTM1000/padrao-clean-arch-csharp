using CleanArchTemplate.Application.Products.Commands.CreateProduct;
using CleanArchTemplate.Domain.Products;
using FluentAssertions;
using NSubstitute;

namespace CleanArchTemplate.Application.Tests.Products.Commands;

public sealed class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();

    [Fact]
    public async Task Creates_product_and_returns_id()
    {
        var sut = new CreateProductCommandHandler(_products);

        var result = await sut.Handle(
            new CreateProductCommand("ABC-1", "Camisa", 50m, "BRL"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _products.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}
