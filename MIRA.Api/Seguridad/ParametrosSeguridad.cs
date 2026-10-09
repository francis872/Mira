namespace MIRA.Api.Seguridad;

/// <summary>Valores fijos de la autenticación; solo Jwt:Secret se configura por entorno.</summary>
public static class ParametrosSeguridad
{
    public const string Emisor = "MIRA.Api";
    public const string Audiencia = "MIRA.Client";
    public const int CostoBcrypt = 12;
    public const int MinutosDeSesion = 60;
}
