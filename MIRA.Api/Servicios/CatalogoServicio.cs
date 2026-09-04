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

    public async Task<CatalogoItem> ObtenerPorIdAsync(string tipo, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ApiException("Id de catálogo inválido.");
        }

        var item = await catalogoRepositorio.ObtenerPorIdAsync(NormalizarTipo(tipo), id, cancellationToken);
        return item ?? throw new ApiException("Registro no encontrado.", StatusCodes.Status404NotFound);
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

    public async Task<CatalogoItem> ActualizarAsync(string tipo, int id, string nombre, bool activo, CancellationToken cancellationToken)
    {
        if (id <= 0 || string.IsNullOrWhiteSpace(nombre))
        {
            throw new ApiException("Id y nombre son requeridos.");
        }

        var item = await catalogoRepositorio.ActualizarAsync(NormalizarTipo(tipo), id, nombre.Trim(), activo, cancellationToken);
        return item ?? throw new ApiException("Registro no encontrado.", StatusCodes.Status404NotFound);
    }

    public async Task DesactivarAsync(string tipo, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ApiException("Id de catálogo inválido.");
        }

        var actualizado = await catalogoRepositorio.DesactivarAsync(NormalizarTipo(tipo), id, cancellationToken);
        if (!actualizado)
        {
            throw new ApiException("Registro no encontrado.", StatusCodes.Status404NotFound);
        }
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
