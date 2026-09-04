namespace MIRA.Api.Modelos;

public sealed class Usuario
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string Rol { get; init; } = "Docente";
}
