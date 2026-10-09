namespace MIRA.Api.Seguridad;
public sealed record UsuarioAuth(long Id,string Correo,string PasswordHash,bool Activo);
public sealed record RolItem(int Id,string Nombre);
public interface IAuthRepository
{
 Task<UsuarioAuth?> BuscarPorCorreoAsync(string correo);
 Task<IReadOnlyList<RolItem>> RolesUsuarioAsync(long id);
 Task<IReadOnlyList<RolItem>> RolesActivosAsync();
 Task<string> CrearAsync(string correo,string hash,int[] roles);
 Task<string?> ActualizarAsync(long id,string correo,int[] roles);
 Task<string?> ConsultarAsync(long id);
 Task<string> ListarAsync();
 Task<bool> InactivarAsync(long id);
 Task<string> BootstrapAdminAsync(string correo, string passwordHash);
}
