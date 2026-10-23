using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Modelos;

public class AreaAplicacion
{
    public int Id { get; set; }
    [StringLength(200, ErrorMessage = "El campo Nombre admite hasta 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

