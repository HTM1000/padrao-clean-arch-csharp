namespace CleanArchTemplate.Domain.Users;

/// <summary>Identificador fortemente tipado de <see cref="User"/>.</summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
}
