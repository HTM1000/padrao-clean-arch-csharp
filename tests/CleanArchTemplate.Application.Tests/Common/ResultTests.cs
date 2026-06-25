using CleanArchTemplate.Application.Common;
using FluentAssertions;

namespace CleanArchTemplate.Application.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_has_value_and_no_error()
    {
        var result = Result<int>.Success(5);
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void Failure_carries_typed_error()
    {
        var result = Result<int>.Failure(Error.NotFound("X.NotFound", "not found"));
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Accessing_value_on_failure_throws()
    {
        var result = Result<int>.Failure(Error.Failure("X", "boom"));
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Implicit_from_value_is_success()
    {
        Result<int> result = 42;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Implicit_from_error_is_failure()
    {
        Result<int> result = Error.Conflict("X.Conflict", "dup");
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Non_generic_success_and_failure()
    {
        Result.Success().IsSuccess.Should().BeTrue();
        Result.Failure(Error.Validation("X", "bad")).IsFailure.Should().BeTrue();
    }
}
