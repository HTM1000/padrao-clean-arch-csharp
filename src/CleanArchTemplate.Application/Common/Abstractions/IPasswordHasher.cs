namespace CleanArchTemplate.Application.Common.Abstractions;

/// <summary>Gera e verifica hashes de senha. Implementado na Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
