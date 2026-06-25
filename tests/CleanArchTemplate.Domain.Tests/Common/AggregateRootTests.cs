using CleanArchTemplate.Domain.Common;
using FluentAssertions;

namespace CleanArchTemplate.Domain.Tests.Common;

public sealed class AggregateRootTests
{
    private sealed record Created : IDomainEvent;

    private sealed class Sample() : AggregateRoot<Guid>(Guid.NewGuid())
    {
        public void Do() => RaiseDomainEvent(new Created());
    }

    [Fact]
    public void Raises_domain_events()
    {
        var sample = new Sample();
        sample.Do();
        sample.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<Created>();
    }

    [Fact]
    public void Clears_domain_events()
    {
        var sample = new Sample();
        sample.Do();
        sample.ClearDomainEvents();
        sample.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Entities_with_different_ids_are_not_equal()
    {
        var a = new Sample();
        var b = new Sample();
        a.Should().NotBe(b);
        a.Should().Be(a);
    }
}
