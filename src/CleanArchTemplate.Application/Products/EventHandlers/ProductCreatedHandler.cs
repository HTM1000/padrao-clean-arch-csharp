using CleanArchTemplate.Application.Common.Messaging;
using CleanArchTemplate.Domain.Products.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchTemplate.Application.Products.EventHandlers;

/// <summary>Exemplo de consumo de evento de domínio. Aqui você dispararia e-mail, integração, etc.</summary>
public sealed partial class ProductCreatedHandler(ILogger<ProductCreatedHandler> logger)
    : INotificationHandler<DomainEventNotification<ProductCreated>>
{
    public Task Handle(DomainEventNotification<ProductCreated> notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        LogProductCreated(logger, notification.DomainEvent.ProductId.Value);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Produto criado: {ProductId}")]
    private static partial void LogProductCreated(ILogger logger, Guid productId);
}
