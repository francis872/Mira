namespace MIRA.Api.Seguridad;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<IReadOnlyList<RolItem>> RolesActivosAsync();
    Task<IReadOnlyList<UsuarioAdminResponse>> ListarUsuariosAsync();
    Task<UsuarioAdminResponse?> ConsultarUsuarioAsync(long id);
    Task<UsuarioAdminResponse> CrearUsuarioAsync(CrearUsuarioRequest request);
    Task<UsuarioAdminResponse> ActualizarUsuarioAsync(long id, ActualizarUsuarioRequest request);
    Task<bool> InactivarUsuarioAsync(long id);
}