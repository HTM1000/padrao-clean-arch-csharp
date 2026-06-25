using CleanArchTemplate.Application.Common;
using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Domain.Users;
using MediatR;

namespace CleanArchTemplate.Application.Users.Commands.Register;

public sealed class RegisterCommandHandler(IUserRepository users, IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var existing = await users.GetByEmailAsync(command.Email, cancellationToken);
        if (existing is not null)
        {
            return Error.Conflict("User.EmailInUse", "E-mail já cadastrado.");
        }

        var user = User.Create(command.Email, passwordHasher.Hash(command.Password));
        await users.AddAsync(user, cancellationToken);
        return user.Id.Value;
    }
}
