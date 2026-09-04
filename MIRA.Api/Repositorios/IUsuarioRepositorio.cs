using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> ObtenerRolesAsync(int usuarioId, CancellationToken cancellationToken);
    Task<bool> AsignarRolAsync(int usuarioId, string rol, CancellationToken cancellationToken);
    Task<bool> RemoverRolAsync(int usuarioId, string rol, CancellationToken cancellationToken);
}
