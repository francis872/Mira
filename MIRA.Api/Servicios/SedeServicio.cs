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
}
