using System.Text.Json.Nodes;
using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios.Seguridad;

public interface ICuentaRepository
{
    Task<Credencial?> Buscar(string correo);
    Task<Credencial?> Sesion(string jti, long id);
    Task RegistrarSesion(string jti, long id, DateTimeOffset expira);
    Task Revocar(string jti, long id);
    Task<JsonNode> Listar();
    Task<JsonNode> Roles();
    Task<JsonNode> Crear(string correo, string hash, int[] roles);
    Task<JsonNode> Editar(long id, string correo, int[] roles);
    Task<bool> Inactivar(long id);
    Task Bootstrap(string correo, string hash);
    Task Restablecer(long id, string hash);
}

public sealed class CuentaRepository(IDbConnectionFactory factory) : ICuentaRepository
{
    private const string Columns = "u.id, u.correo, u.password_hash, u.activo, ARRAY(SELECT r.nombre FROM usuario_rol ur JOIN rol r ON r.id=ur.rol_id WHERE ur.usuario_id=u.id AND r.activo ORDER BY r.nombre) AS roles";

    public async Task<Credencial?> Buscar(string correo)
    {
        using var db = factory.CreateConnection();
        return await db.QuerySingleOrDefaultAsync<Credencial>($"SELECT {Columns} FROM usuario u WHERE u.correo=@correo", new { correo });
    }
    public async Task<Credencial?> Sesion(string jti, long id)
    {
        using var db = factory.CreateConnection();
        return await db.QuerySingleOrDefaultAsync<Credencial>($"SELECT {Columns} FROM usuario u JOIN sesion_acceso s ON s.usuario_id=u.id WHERE s.jti=@jti AND u.id=@id AND s.expira_en>now() AND u.activo", new { jti, id });
    }
    public async Task RegistrarSesion(string jti, long id, DateTimeOffset expira)
    {
        using var db = factory.CreateConnection();
        await db.ExecuteAsync("DELETE FROM sesion_acceso WHERE expira_en<=now(); INSERT INTO sesion_acceso(jti,usuario_id,expira_en) VALUES (@jti,@id,@expira)", new { jti, id, expira });
    }
    public async Task Revocar(string jti, long id)
    {
        using var db = factory.CreateConnection();
        await db.ExecuteAsync("DELETE FROM sesion_acceso WHERE jti=@jti AND usuario_id=@id", new { jti, id });
    }
    private async Task<JsonNode> Json(string sql, object? args = null)
    {
        using var db = factory.CreateConnection();
        var result = await db.ExecuteScalarAsync<string>(sql, args);
        return JsonNode.Parse(result ?? "null")!;
    }
    public Task<JsonNode> Listar() => Json("SELECT fn_usuario_listar()::text");
    public Task<JsonNode> Roles() => Json("SELECT COALESCE(jsonb_agg(jsonb_build_object('id',id,'nombre',nombre) ORDER BY id),'[]')::text FROM rol WHERE activo");
    public Task<JsonNode> Crear(string correo, string hash, int[] roles) => Json("CALL sp_usuario_crear(@correo,@hash,CAST(@roles AS jsonb),NULL)", new { correo, hash, roles=System.Text.Json.JsonSerializer.Serialize(roles) });
    public Task<JsonNode> Editar(long id, string correo, int[] roles) => Json("CALL sp_v3_usuario_actualizar(@id,@correo,CAST(@roles AS jsonb),NULL)", new { id, correo, roles=System.Text.Json.JsonSerializer.Serialize(roles) });
    public async Task<bool> Inactivar(long id)
    {
        using var db = factory.CreateConnection();
        return await db.ExecuteScalarAsync<bool>("CALL sp_usuario_inactivar(@id,NULL)", new { id });
    }
    public async Task Bootstrap(string correo, string hash) => _ = await Json("CALL sp_usuario_bootstrap_admin(@correo,@hash,NULL)", new { correo, hash });
    public async Task Restablecer(long id, string hash)
    {
        using var db = factory.CreateConnection();
        await db.ExecuteAsync("CALL sp_v3_usuario_password(@id,@hash)", new { id, hash });
    }
}
