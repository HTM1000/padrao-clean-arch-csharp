using CleanArchTemplate.Domain.Common;
using CleanArchTemplate.Domain.Products.Events;
using CleanArchTemplate.Domain.Products.ValueObjects;

namespace CleanArchTemplate.Domain.Products;

/// <summary>Agregado raiz de produto.</summary>
public sealed class Product : AggregateRoot<ProductId>
{
    private Product(ProductId id, Sku sku, string name, Money price) : base(id)
    {
        Sku = sku;
        Name = name;
        Price = price;
    }

    // Construtor para o materializador do EF Core; não dispara eventos.
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1144:Unused private types or members should be removed",
        Justification = "Usado pelo materializador do EF Core via reflexão.")]
    private Product() : base(default)
    {
        Sku = null!;
        Name = null!;
        Price = null!;
    }

    public Sku Sku { get; private set; }

    public string Name { get; private set; }

    public Money Price { get; private set; }

    public static Product Create(Sku sku, string name, Money price)
    {
        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(price);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Nome do produto é obrigatório.");
        }

        var product = new Product(ProductId.New(), sku, name.Trim(), price);
        product.RaiseDomainEvent(new ProductCreated(product.Id, sku, price));
        return product;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Nome do produto é obrigatório.");
        }

        Name = name.Trim();
    }
}
