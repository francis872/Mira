using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Peticiones;

public sealed class AsignarRolRequest
{
    [Required]
    [MaxLength(50)]
    public string Rol { get; init; } = string.Empty;
}
