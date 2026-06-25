namespace CleanArchTemplate.Domain.Common;

/// <summary>Lançada quando uma invariante de domínio é violada.</summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
