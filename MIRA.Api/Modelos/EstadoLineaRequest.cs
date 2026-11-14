using System.ComponentModel.DataAnnotations;
namespace MIRA.Api.Modelos;
public sealed class EstadoLineaRequest
{
    [Required] public bool? Activo { get; set; }
}
