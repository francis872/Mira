using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Modelos;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(254)] string Correo,
    [Required, MaxLength(128)] string Password);

public sealed record CrearCuentaRequest(
    [Required, EmailAddress, MaxLength(254)] string Correo,
    [Required, MinLength(12), MaxLength(128)] string Password,
    [Required, MinLength(1)] int[] Roles);

public sealed record EditarCuentaRequest(
    [Required, EmailAddress, MaxLength(254)] string Correo,
    [Required, MinLength(1)] int[] Roles);

public sealed record Cuenta(long Id, string Correo, bool Activo, string[] Roles);
public sealed record PasswordRequest([Required, MinLength(12), MaxLength(128)] string Password);
// Solo el repositorio y el servicio de autenticación reciben el hash.
public sealed record Credencial(long Id, string Correo, string PasswordHash, bool Activo, string[] Roles)
{
    // Npgsql describe los arrays como System.Array; Dapper debe asignar propiedades,
    // en lugar de elegir el constructor posicional string[].
    public Credencial() : this(0, "", "", false, []) { }
}
public sealed record SesionRespuesta(string Token, DateTimeOffset ExpiraEn, Cuenta Usuario);
