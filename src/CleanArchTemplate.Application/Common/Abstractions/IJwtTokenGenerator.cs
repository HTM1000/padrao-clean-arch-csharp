using CleanArchTemplate.Domain.Users;

namespace CleanArchTemplate.Application.Common.Abstractions;

/// <summary>Gera tokens JWT para usuários autenticados. Implementado na Infrastructure.</summary>
public interface IJwtTokenGenerator
{
    string Generate(User user);
}
