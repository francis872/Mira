using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken);
}
