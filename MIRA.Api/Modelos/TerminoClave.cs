using System.ComponentModel.DataAnnotations;

namespace MIRA.Api.Modelos;

public class TerminoClave
{
    [StringLength(150, ErrorMessage = "El campo Termino admite hasta 150 caracteres.")]
    public string Termino { get; set; } = string.Empty;
    [StringLength(150, ErrorMessage = "El campo TerminoIngles admite hasta 150 caracteres.")]
    public string? TerminoIngles { get; set; }
    public bool Activo { get; set; } = true;
}

