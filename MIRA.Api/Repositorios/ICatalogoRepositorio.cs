using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public interface ICatalogoRepositorio
{
    Task<IReadOnlyList<CatalogoItem>> ListarAsync(string tipo, CancellationToken cancellationToken);
    Task<CatalogoItem?> ObtenerPorIdAsync(string tipo, int id, CancellationToken cancellationToken);
    Task<CatalogoItem> CrearAsync(string tipo, string nombre, CancellationToken cancellationToken);
    Task<CatalogoItem?> ActualizarAsync(string tipo, int id, string nombre, bool activo, CancellationToken cancellationToken);
    Task<bool> DesactivarAsync(string tipo, int id, CancellationToken cancellationToken);
}
