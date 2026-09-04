using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Peticiones;

public sealed class CatalogoItemRequest
{
    [Required]
    [MaxLength(160)]
    public string Nombre { get; init; } = string.Empty;
}
