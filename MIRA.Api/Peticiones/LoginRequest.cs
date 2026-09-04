using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Peticiones;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Correo { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;
}
