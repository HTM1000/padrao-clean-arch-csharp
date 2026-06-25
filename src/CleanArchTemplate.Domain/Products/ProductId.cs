namespace CleanArchTemplate.Domain.Products;

/// <summary>Identificador fortemente tipado de <see cref="Product"/>.</summary>
public readonly record struct ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.NewGuid());
}
