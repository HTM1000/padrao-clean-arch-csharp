using CleanArchTemplate.Application.Common.Messaging;

namespace CleanArchTemplate.Application.Users.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<string>;
