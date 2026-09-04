using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public interface ISedeRepositorio
{
    Task<IReadOnlyList<Sede>> ListarAsync(CancellationToken cancellationToken);
    Task<Sede> CrearAsync(string nombre, string ciudad, CancellationToken cancellationToken);
}
