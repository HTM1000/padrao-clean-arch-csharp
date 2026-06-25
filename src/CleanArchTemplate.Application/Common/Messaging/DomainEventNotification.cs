using CleanArchTemplate.Domain.Common;
using MediatR;

namespace CleanArchTemplate.Application.Common.Messaging;

/// <summary>Adaptador que transporta um evento de domínio como notificação do MediatR.</summary>
/// <remarks>Mantém o Domain livre de MediatR: a ponte vive na Application.</remarks>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
