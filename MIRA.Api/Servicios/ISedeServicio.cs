using MIRA.Api.Modelos;

namespace MIRA.Api.Servicios;

public interface ISedeServicio
{
    Task<IReadOnlyList<Sede>> ListarAsync(CancellationToken cancellationToken);
    Task<Sede> CrearAsync(string nombre, string ciudad, CancellationToken cancellationToken);
    Task<Sede> ActualizarAsync(int id, string nombre, string ciudad, bool activa, CancellationToken cancellationToken);
    Task DesactivarAsync(int id, CancellationToken cancellationToken);
}
