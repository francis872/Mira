using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Modelos;

public class ObjetivoDesarrolloSostenible
{
    public int Id { get; set; }
    [StringLength(200, ErrorMessage = "El campo Nombre admite hasta 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [StringLength(100, ErrorMessage = "El campo Categoria admite hasta 100 caracteres.")]
    public string Categoria { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

