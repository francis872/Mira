using MIRA.Api.Modelos;

namespace MIRA.Api.Servicios;

public interface ICatalogoServicio
{
    Task<IReadOnlyList<CatalogoItem>> ListarAsync(string tipo, CancellationToken cancellationToken);
    Task<CatalogoItem> CrearAsync(string tipo, string nombre, CancellationToken cancellationToken);
}
