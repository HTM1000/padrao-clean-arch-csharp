using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Domain.Users;
using MediatR;

namespace CleanArchTemplate.Application.Users.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator)
    : IRequestHandler<LoginCommand, Result<string>>
{
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "E-mail ou senha inválidos.");

    public async Task<Result<string>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return InvalidCredentials;
        }

        return tokenGenerator.Generate(user);
    }
}
