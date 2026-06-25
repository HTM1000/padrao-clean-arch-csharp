namespace CleanArchTemplate.Infrastructure.Authentication;

/// <summary>Configuração do JWT (seção <c>Jwt</c> do appsettings).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
