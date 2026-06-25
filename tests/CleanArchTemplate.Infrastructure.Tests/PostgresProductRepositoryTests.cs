using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.ValueObjects;
using CleanArchTemplate.Infrastructure.Persistence.Repositories;
using CleanArchTemplate.Infrastructure.Tests.Fixtures;
using FluentAssertions;

namespace CleanArchTemplate.Infrastructure.Tests;

[Trait("Category", "Integration")]
public sealed class PostgresProductRepositoryTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Persists_and_reads_product()
    {
        var repository = new ProductRepository(fixture.Context);
        var product = Product.Create(Sku.Create("ABC-1"), "Camisa", Money.Create(50m, "BRL"));

        await repository.AddAsync(product);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var found = await repository.GetByIdAsync(product.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Camisa");
        found.Sku.Value.Should().Be("ABC-1");
        found.Price.Amount.Should().Be(50m);
        found.Price.Currency.Should().Be("BRL");
    }
}
