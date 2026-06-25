using CleanArchTemplate.Domain.Common;
using FluentAssertions;

namespace CleanArchTemplate.Domain.Tests.Common;

public sealed class ValueObjectTests
{
    private sealed class Pair(int a, int b) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return a;
            yield return b;
        }
    }

    [Fact]
    public void Equal_when_components_match()
    {
        new Pair(1, 2).Should().Be(new Pair(1, 2));
        (new Pair(1, 2) == new Pair(1, 2)).Should().BeTrue();
    }

    [Fact]
    public void NotEqual_when_components_differ()
    {
        new Pair(1, 2).Should().NotBe(new Pair(2, 1));
        (new Pair(1, 2) != new Pair(2, 1)).Should().BeTrue();
    }

    [Fact]
    public void Same_components_share_hashcode()
    {
        new Pair(1, 2).GetHashCode().Should().Be(new Pair(1, 2).GetHashCode());
    }
}
