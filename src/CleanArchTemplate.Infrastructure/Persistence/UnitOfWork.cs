using CleanArchTemplate.Application.Common.Abstractions;
using CleanArchTemplate.Application.Common.Messaging;
using CleanArchTemplate.Domain.Common;
using MediatR;

namespace CleanArchTemplate.Infrastructure.Persistence;

/// <summary>Confirma as mudanças e dispara os eventos de domínio coletados pelos agregados.</summary>
public sealed class UnitOfWork(AppDbContext context, IPublisher publisher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

        var affected = await context.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await publisher.Publish(notification, cancellationToken);
        }

        return affected;
    }
}
