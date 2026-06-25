namespace CleanArchTemplate.Domain.Users;

/// <summary>Coleção persistente de usuários (port; implementado na Infrastructure).</summary>
public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
