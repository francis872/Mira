using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Peticiones;

public sealed class CatalogoItemRequest
{
    [Required]
    [MaxLength(160)]
    public string Nombre { get; init; } = string.Empty;

    public bool Activo { get; init; } = true;
}

public sealed class CatalogoItemPatchRequest
{
    [MaxLength(160)]
    public string? Nombre { get; init; }

    public bool? Activo { get; init; }
}
