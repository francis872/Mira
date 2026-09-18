using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public interface ISedeRepositorio
{
    Task<IReadOnlyList<Sede>> ListarAsync(CancellationToken cancellationToken);
    Task<Sede?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Sede> CrearAsync(string nombre, string ciudad, CancellationToken cancellationToken);
    Task<Sede?> ActualizarAsync(int id, string nombre, string ciudad, bool activa, CancellationToken cancellationToken);
    Task<bool> DesactivarAsync(int id, CancellationToken cancellationToken);
}
