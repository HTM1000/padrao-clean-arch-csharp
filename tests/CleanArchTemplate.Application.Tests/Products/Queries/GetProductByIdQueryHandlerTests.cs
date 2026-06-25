using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Application.Products.Queries.GetProductById;
using CleanArchTemplate.Domain.Products;
using CleanArchTemplate.Domain.Products.ValueObjects;
using CleanArchTemplate.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Tests.Products.Queries;

public sealed class GetProductByIdQueryHandlerTests
{
    private sealed class TestDbContext(DbContextOptions options) : DbContext(options), IApplicationDbContext
    {
        public DbSet<Product> Products => Set<Product>();

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(product => product.Id);
                entity.Property(product => product.Id)
                    .HasConversion(id => id.Value, value => new ProductId(value));
                entity.OwnsOne(product => product.Sku);
                entity.OwnsOne(product => product.Price);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Id)
                    .HasConversion(id => id.Value, value => new UserId(value));
            });
        }
    }

    private static IApplicationDbContext ContextWith(params Product[] seed)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new TestDbContext(options);
        context.Products.AddRange(seed);
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task Returns_dto_when_found()
    {
        var product = Product.Create(Sku.Create("ABC-1"), "Camisa", Money.Create(50m, "BRL"));
        var sut = new GetProductByIdQueryHandler(ContextWith(product));

        var result = await sut.Handle(new GetProductByIdQuery(product.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Camisa");
        result.Value.Price.Should().Be(50m);
    }

    [Fact]
    public async Task Returns_notfound_when_missing()
    {
        var sut = new GetProductByIdQueryHandler(ContextWith());

        var result = await sut.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
