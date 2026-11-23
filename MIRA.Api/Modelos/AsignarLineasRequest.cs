using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Modelos;

public sealed class AsignarLineasRequest
{
    [Range(1,int.MaxValue)] public int PadreId { get; set; }
    [Required,MinLength(1),MaxLength(200)] public int[] Lineas { get; set; } = [];
}
