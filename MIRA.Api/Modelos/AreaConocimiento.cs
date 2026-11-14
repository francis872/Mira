using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Modelos;

public class AreaConocimiento
{
    public int Id { get; set; }
    [StringLength(150, ErrorMessage = "El campo GranArea admite hasta 150 caracteres.")]
    public string GranArea { get; set; } = string.Empty;
    [StringLength(150, ErrorMessage = "El campo Area admite hasta 150 caracteres.")]
    public string Area { get; set; } = string.Empty;
    [StringLength(150, ErrorMessage = "El campo Disciplina admite hasta 150 caracteres.")]
    public string Disciplina { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

