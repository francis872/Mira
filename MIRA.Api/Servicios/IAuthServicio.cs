namespace MIRA.Api.Servicios;

public interface IAuthServicio
{
    Task<AuthResultado> LoginAsync(string correo, string password, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> ObtenerRolesAsync(int usuarioId, CancellationToken cancellationToken);
    Task AsignarRolAsync(int usuarioId, string rol, CancellationToken cancellationToken);
    Task RemoverRolAsync(int usuarioId, string rol, CancellationToken cancellationToken);
}
