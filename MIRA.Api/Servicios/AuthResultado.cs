namespace MIRA.Api.Servicios;

public sealed class AuthResultado
{
    public required string Token { get; init; }
    public required DateTime ExpiraEnUtc { get; init; }
    public required string Nombre { get; init; }
    public required string Correo { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
}
