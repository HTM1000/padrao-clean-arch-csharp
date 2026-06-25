namespace CleanArchTemplate.Application.Common.Abstractions;

/// <summary>Confirma as mudanças do agregado e dispara os eventos de domínio. Implementada na Infrastructure.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
