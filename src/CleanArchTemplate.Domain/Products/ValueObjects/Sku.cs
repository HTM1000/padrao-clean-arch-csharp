using CleanArchTemplate.Domain.Common;

namespace CleanArchTemplate.Domain.Products.ValueObjects;

/// <summary>Stock Keeping Unit — código de identificação do produto. Imutável e normalizado.</summary>
public sealed class Sku : ValueObject
{
    private Sku(string value) => Value = value;

    // Construtor para o materializador do EF Core (escreve via setter privado).
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1144:Unused private types or members should be removed",
        Justification = "Usado pelo materializador do EF Core via reflexão.")]
    private Sku() => Value = string.Empty;

    public string Value { get; private set; }

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("SKU é obrigatório.");
        }

        return new Sku(value.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
