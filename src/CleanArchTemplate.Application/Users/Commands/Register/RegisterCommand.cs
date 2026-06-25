using CleanArchTemplate.Application.Common.Messaging;

namespace CleanArchTemplate.Application.Users.Commands.Register;

public sealed record RegisterCommand(string Email, string Password) : ICommand<Guid>;
