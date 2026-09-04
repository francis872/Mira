namespace MIRA.Api.Configuracion;

public sealed class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "mira-api";
    public string Audience { get; set; } = "mira-frontend";
    public int ExpirationHours { get; set; } = 24;
}
