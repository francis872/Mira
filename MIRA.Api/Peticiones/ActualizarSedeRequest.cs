using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Peticiones;

public sealed class ActualizarSedeRequest
{
    [Required]
    [MaxLength(120)]
    public string Nombre { get; init; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Ciudad { get; init; } = string.Empty;

    public bool Activa { get; init; } = true;
}

public sealed class ActualizarSedeParcialRequest
{
    [MaxLength(120)]
    public string? Nombre { get; init; }

    [MaxLength(120)]
    public string? Ciudad { get; init; }

    public bool? Activa { get; init; }
}
