using MIRA.Api.Excepciones;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios;

namespace MIRA.Api.Servicios;

public sealed class CatalogoServicio(ICatalogoRepositorio catalogoRepositorio) : ICatalogoServicio
{
    private static readonly HashSet<string> TiposValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "areas-conocimiento",
        "ods",
        "aplicaciones",
        "palabras-clave"
    };

    public Task<IReadOnlyList<CatalogoItem>> ListarAsync(string tipo, CancellationToken cancellationToken)
    {
        var normalizado = NormalizarTipo(tipo);
        return catalogoRepositorio.ListarAsync(normalizado, cancellationToken);
    }

    public Task<CatalogoItem> CrearAsync(string tipo, string nombre, CancellationToken cancellationToken)
    {
        var normalizado = NormalizarTipo(tipo);
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ApiException("El nombre del catálogo es requerido.");
        }

        return catalogoRepositorio.CrearAsync(normalizado, nombre.Trim(), cancellationToken);
    }

    private static string NormalizarTipo(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo) || !TiposValidos.Contains(tipo))
        {
            throw new ApiException("Tipo de catálogo inválido. Usa: areas-conocimiento, ods, aplicaciones, palabras-clave.", StatusCodes.Status404NotFound);
        }

        return tipo.ToLowerInvariant();
    }
}
