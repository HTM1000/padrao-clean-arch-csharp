using CleanArchTemplate.Domain.Common;
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.Events;
using CleanArchTemplate.Domain.Products.ValueObjects;
using FluentAssertions;

namespace CleanArchTemplate.Domain.Tests.Products;

public sealed class ProductTests
{
    private static Product NewProduct() =>
        Product.Create(Sku.Create("ABC-1"), "Camisa", Money.Create(50m, "BRL"));

    [Fact]
    public void Create_sets_properties()
    {
        var product = NewProduct();
        product.Name.Should().Be("Camisa");
        product.Sku.Value.Should().Be("ABC-1");
        product.Price.Amount.Should().Be(50m);
    }

    [Fact]
    public void Create_raises_product_created_event()
    {
        NewProduct().DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ProductCreated>();
    }

    [Fact]
    public void Create_rejects_blank_name()
    {
        var act = () => Product.Create(Sku.Create("ABC-1"), " ", Money.Create(50m, "BRL"));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_changes_name()
    {
        var product = NewProduct();
        product.Rename("Camiseta");
        product.Name.Should().Be("Camiseta");
    }
}
