namespace CleanArchTemplate.Domain.Common;

/// <summary>Base para entidades: identidade própria, igualdade por Id e tipo.</summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : struct
{
    protected Entity(TId id) => Id = id;

    public TId Id { get; }

    public bool Equals(Entity<TId>? other) =>
        other is not null && GetType() == other.GetType() && Id.Equals(other.Id);

    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
