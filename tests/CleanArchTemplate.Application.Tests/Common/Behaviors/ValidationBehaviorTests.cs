using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Behaviors;
using CleanArchTemplate.Application.Common.Messaging;
using FluentAssertions;
using FluentValidation;
using MediatR;

namespace CleanArchTemplate.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record Cmd(int X) : ICommand<int>;

    private sealed class CmdValidator : AbstractValidator<Cmd>
    {
        public CmdValidator() => RuleFor(c => c.X).GreaterThan(0);
    }

    [Fact]
    public async Task Returns_validation_failure_when_invalid()
    {
        var behavior = new ValidationBehavior<Cmd, Result<int>>([new CmdValidator()]);
        RequestHandlerDelegate<Result<int>> next = () => Task.FromResult(Result<int>.Success(1));

        var result = await behavior.Handle(new Cmd(0), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Calls_next_when_valid()
    {
        var behavior = new ValidationBehavior<Cmd, Result<int>>([new CmdValidator()]);
        RequestHandlerDelegate<Result<int>> next = () => Task.FromResult(Result<int>.Success(99));

        var result = await behavior.Handle(new Cmd(5), next, CancellationToken.None);

        result.Value.Should().Be(99);
    }

    [Fact]
    public async Task Calls_next_when_no_validators()
    {
        var behavior = new ValidationBehavior<Cmd, Result<int>>([]);
        RequestHandlerDelegate<Result<int>> next = () => Task.FromResult(Result<int>.Success(7));

        var result = await behavior.Handle(new Cmd(0), next, CancellationToken.None);

        result.Value.Should().Be(7);
    }
}
