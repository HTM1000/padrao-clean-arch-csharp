namespace CleanArchTemplate.Domain.Common;

/// <summary>Contrato não-genérico para iterar eventos de domínio de agregados a partir da Infrastructure.</summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
