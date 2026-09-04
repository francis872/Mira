using MIRA.Api.Excepciones;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios;

namespace MIRA.Api.Servicios;

public sealed class SedeServicio(ISedeRepositorio sedeRepositorio) : ISedeServicio
{
    public Task<IReadOnlyList<Sede>> ListarAsync(CancellationToken cancellationToken)
        => sedeRepositorio.ListarAsync(cancellationToken);

    public async Task<Sede> CrearAsync(string nombre, string ciudad, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(ciudad))
        {
            throw new ApiException("Nombre y ciudad son requeridos.");
        }

        return await sedeRepositorio.CrearAsync(nombre.Trim(), ciudad.Trim(), cancellationToken);
    }

    public async Task<Sede> ActualizarAsync(int id, string nombre, string ciudad, bool activa, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ApiException("Id de sede inválido.");
        }

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(ciudad))
        {
            throw new ApiException("Nombre y ciudad son requeridos.");
        }

        var sede = await sedeRepositorio.ActualizarAsync(id, nombre.Trim(), ciudad.Trim(), activa, cancellationToken);
        return sede ?? throw new ApiException("Sede no encontrada.", StatusCodes.Status404NotFound);
    }

    public async Task DesactivarAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ApiException("Id de sede inválido.");
        }

        var desactivada = await sedeRepositorio.DesactivarAsync(id, cancellationToken);
        if (!desactivada)
        {
            throw new ApiException("Sede no encontrada.", StatusCodes.Status404NotFound);
        }
    }
}
