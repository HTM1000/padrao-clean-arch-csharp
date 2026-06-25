using System.Reflection;
using CleanArchTemplate.Domain.Common;
using FluentAssertions;
using NetArchTest.Rules;

namespace CleanArchTemplate.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string ApplicationNamespace = "CleanArchTemplate.Application";
    private const string InfrastructureNamespace = "CleanArchTemplate.Infrastructure";
    private const string ApiNamespace = "CleanArchTemplate.Api";

    private static readonly Assembly DomainAssembly = typeof(AggregateRoot<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_should_not_depend_on_other_layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                ApplicationNamespace,
                InfrastructureNamespace,
                ApiNamespace,
                "MediatR",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BecauseOf(result));
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BecauseOf(result));
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BecauseOf(result));
    }

    private static string BecauseOf(TestResult result) =>
        $"violações: {string.Join(", ", result.FailingTypeNames ?? [])}";
}
