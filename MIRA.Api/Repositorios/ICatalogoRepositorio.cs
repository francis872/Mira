using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public interface ICatalogoRepositorio
{
    Task<IReadOnlyList<CatalogoItem>> ListarAsync(string tipo, CancellationToken cancellationToken);
    Task<CatalogoItem> CrearAsync(string tipo, string nombre, CancellationToken cancellationToken);
}
