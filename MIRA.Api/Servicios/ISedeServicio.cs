using MIRA.Api.Modelos;

namespace MIRA.Api.Servicios;

public interface ISedeServicio
{
    Task<IReadOnlyList<Sede>> ListarAsync(CancellationToken cancellationToken);
    Task<Sede> CrearAsync(string nombre, string ciudad, CancellationToken cancellationToken);
}
