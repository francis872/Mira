using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Modelos;

public class Universidad
{
    public int Id { get; set; }
    [StringLength(200, ErrorMessage = "El campo Nombre admite hasta 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [StringLength(100, ErrorMessage = "El campo Tipo admite hasta 100 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [StringLength(100, ErrorMessage = "El campo Ciudad admite hasta 100 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

