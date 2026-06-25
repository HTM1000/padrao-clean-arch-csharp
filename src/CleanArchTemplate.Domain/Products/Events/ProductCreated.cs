using CleanArchTemplate.Domain.Common;
using CleanArchTemplate.Domain.Products.ValueObjects;

namespace CleanArchTemplate.Domain.Products.Events;

/// <summary>Disparado quando um novo produto é criado.</summary>
public sealed record ProductCreated(ProductId ProductId, Sku Sku, Money Price) : IDomainEvent;
