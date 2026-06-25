using System.Reflection;
using FluentAssertions;
using MediatR;
using NetArchTest.Rules;

namespace CleanArchTemplate.Architecture.Tests;

public sealed class NamingConventionTests
{
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;

    [Fact]
    public void Request_handlers_should_be_sealed_and_named_Handler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .BeSealed()
            .And()
            .HaveNameEndingWith("Handler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"violações: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Validators_should_be_sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"violações: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
