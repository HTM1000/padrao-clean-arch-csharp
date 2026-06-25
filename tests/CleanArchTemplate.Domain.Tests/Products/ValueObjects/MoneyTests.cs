using CleanArchTemplate.Domain.Common;
using CleanArchTemplate.Domain.Products.ValueObjects;
using FluentAssertions;

namespace CleanArchTemplate.Domain.Tests.Products.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_rejects_negative_amount()
    {
        var act = () => Money.Create(-1m, "BRL");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_rejects_blank_currency()
    {
        var act = () => Money.Create(1m, " ");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_normalizes_currency_to_upper()
    {
        Money.Create(10m, "brl").Currency.Should().Be("BRL");
    }

    [Fact]
    public void Equal_by_value()
    {
        Money.Create(10m, "BRL").Should().Be(Money.Create(10m, "BRL"));
    }
}
