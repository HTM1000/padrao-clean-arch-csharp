using CleanArchTemplate.Domain.Common;

namespace CleanArchTemplate.Domain.Products.ValueObjects;

/// <summary>Valor monetário (montante + moeda). Imutável e validado.</summary>
public sealed class Money : ValueObject
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // Construtor para o materializador do EF Core (escreve via setters privados).
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1144:Unused private types or members should be removed",
        Justification = "Usado pelo materializador do EF Core via reflexão.")]
    private Money()
    {
        Currency = string.Empty;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("Valor monetário não pode ser negativo.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Moeda é obrigatória.");
        }

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
