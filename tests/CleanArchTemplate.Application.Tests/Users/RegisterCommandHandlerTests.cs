using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Application.Users.Commands.Register;
using CleanArchTemplate.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace CleanArchTemplate.Application.Tests.Users;

public sealed class RegisterCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    [Fact]
    public async Task Registers_user_and_returns_id()
    {
        _users.GetByEmailAsync("a@a.com", Arg.Any<CancellationToken>()).Returns((User?)null);
        _hasher.Hash("password1").Returns("hashed");
        var sut = new RegisterCommandHandler(_users, _hasher);

        var result = await sut.Handle(new RegisterCommand("a@a.com", "password1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _users.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Fails_when_email_already_exists()
    {
        _users.GetByEmailAsync("a@a.com", Arg.Any<CancellationToken>())
            .Returns(User.Create("a@a.com", "hash"));
        var sut = new RegisterCommandHandler(_users, _hasher);

        var result = await sut.Handle(new RegisterCommand("a@a.com", "password1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}
