using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Peticiones;

public sealed class CrearSedeRequest
{
    [Required]
    [MaxLength(120)]
    public string Nombre { get; init; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Ciudad { get; init; } = string.Empty;
}
