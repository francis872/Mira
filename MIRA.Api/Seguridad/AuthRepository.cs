using Dapper;
using MIRA.Api.Configuracion;
namespace MIRA.Api.Seguridad;
public sealed class AuthRepository : IAuthRepository
{
 private readonly IDbConnectionFactory _factory;
 public AuthRepository(IDbConnectionFactory factory) => _factory=factory;
 public async Task<UsuarioAuth?> BuscarPorCorreoAsync(string correo)
 {
  using var db=_factory.CreateConnection();
  return await db.QuerySingleOrDefaultAsync<UsuarioAuth>(
   "SELECT id,correo,password_hash AS PasswordHash,activo FROM usuario WHERE correo=lower(trim(@correo))",
   new {correo});
 }
 public async Task<IReadOnlyList<RolItem>> RolesUsuarioAsync(long id)
 {
  using var db=_factory.CreateConnection();
  var result=await db.QueryAsync<RolItem>(
    "SELECT r.id,r.nombre FROM rol r JOIN usuario_rol ur ON ur.rol_id=r.id WHERE ur.usuario_id=@id AND r.activo ORDER BY r.id",
    new{id});
  return result.AsList();
 }
 public async Task<IReadOnlyList<RolItem>> RolesActivosAsync()
 {
  using var db=_factory.CreateConnection();
  return (await db.QueryAsync<RolItem>("SELECT id,nombre FROM rol WHERE activo ORDER BY nombre")).AsList();
 }
 public async Task<string> CrearAsync(string correo,string hash,int[] roles)
 {
  using var db=_factory.CreateConnection();
  return await db.QuerySingleAsync<string>(
   "SELECT fn_usuario_crear(@correo,@hash,CAST(@roles AS jsonb))::text",
   new {correo,hash,roles=System.Text.Json.JsonSerializer.Serialize(roles)});
 }
 public async Task<string?> ActualizarAsync(long id,string correo,int[] roles)
 {
  using var db=_factory.CreateConnection();
  return await db.QuerySingleAsync<string>(
   "SELECT fn_usuario_actualizar(@id,@correo,CAST(@roles AS jsonb))::text",
   new{id,correo,roles=System.Text.Json.JsonSerializer.Serialize(roles)});
 }
 public async Task<string?> ConsultarAsync(long id)
 {
  using var db=_factory.CreateConnection();
  return await db.QuerySingleOrDefaultAsync<string>("SELECT fn_usuario_consultar(@id)::text",new{id});
 }
 public async Task<string> ListarAsync()
 {
  using var db=_factory.CreateConnection();
  return await db.QuerySingleAsync<string>("SELECT fn_usuario_listar()::text");
 }
 public async Task<bool> InactivarAsync(long id)
 {
  using var db=_factory.CreateConnection();
  return await db.QuerySingleAsync<bool>("SELECT fn_usuario_inactivar(@id)",new{id});
 }
}
