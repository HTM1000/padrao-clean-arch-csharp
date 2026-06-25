using CleanArchTemplate.Domain.Common;

namespace CleanArchTemplate.Domain.Users;

/// <summary>Agregado raiz de usuário para autenticação.</summary>
public sealed class User : AggregateRoot<UserId>
{
    private User(UserId id, string email, string passwordHash) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
    }

    // Construtor para o materializador do EF Core.
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1144:Unused private types or members should be removed",
        Justification = "Usado pelo materializador do EF Core via reflexão.")]
    private User() : base(default)
    {
        Email = null!;
        PasswordHash = null!;
    }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public static User Create(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("E-mail é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Hash de senha é obrigatório.");
        }

        return new User(UserId.New(), email.Trim().ToLowerInvariant(), passwordHash);
    }
}
