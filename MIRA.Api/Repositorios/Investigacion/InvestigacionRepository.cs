using System.Text.Json.Nodes;
using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios.Investigacion;

public sealed class InvestigacionRepository(IDbConnectionFactory factory)
{
    private async Task<JsonNode?> Consultar(string sql, object? parameters = null)
    {
        using var connection = factory.CreateConnection();
        var json = await connection.QuerySingleOrDefaultAsync<string>(sql, parameters);
        return json is null ? null : JsonNode.Parse(json);
    }

    // Only static metadata instances may select a function. All values remain parameters.
    private static string Nombre(RecursoInvestigacion recurso)
    {
        if (!ReferenceEquals(RecursosInvestigacion.Buscar(recurso.Nombre), recurso))
            throw new InvalidOperationException("Recurso no registrado");
        return recurso.Nombre;
    }

    public Task<JsonNode?> Listar(RecursoInvestigacion recurso) =>
        Consultar($"SELECT public.sp_listar_v2_{Nombre(recurso)}();");
    public Task<JsonNode?> Obtener(RecursoInvestigacion recurso, int id) =>
        Consultar($"SELECT public.sp_consultar_v2_{Nombre(recurso)}(@Id);", new { Id=id });
    public Task<JsonNode?> ObtenerPuente(RecursoInvestigacion recurso, int izquierda, int derecha) =>
        Consultar($"SELECT public.sp_consultar_v2_{Nombre(recurso)}(@Izquierda,@Derecha);", new { Izquierda=izquierda, Derecha=derecha });
    public Task<JsonNode?> Crear(RecursoInvestigacion recurso, JsonObject datos) =>
        Consultar($"SELECT public.sp_crear_v2_{Nombre(recurso)}(CAST(@Datos AS jsonb));", new { Datos=datos.ToJsonString() });
    public Task<JsonNode?> Actualizar(RecursoInvestigacion recurso, int id, JsonObject datos) =>
        Consultar($"SELECT public.sp_actualizar_v2_{Nombre(recurso)}(@Id,CAST(@Datos AS jsonb));", new { Id=id, Datos=datos.ToJsonString() });
    public async Task<bool> Eliminar(RecursoInvestigacion recurso, int id, int? derecha = null)
    {
        using var connection = factory.CreateConnection();
        var argumentos = recurso.EsPuente ? "@Id,@Derecha" : "@Id";
        return await connection.QuerySingleAsync<bool>($"SELECT public.sp_eliminar_v2_{Nombre(recurso)}({argumentos});", new { Id=id, Derecha=derecha });
    }
    public Task<JsonNode?> Usuarios() => Consultar("SELECT public.sp_referencias_v2_usuarios();");
    public Task<JsonNode?> Ficha(int id) => Consultar("SELECT public.sp_consultar_v2_ficha_grupo(@Id);", new { Id=id });
    public Task<JsonNode?> GuardarFicha(int? id, JsonObject datos) =>
        Consultar("SELECT public.sp_guardar_v2_ficha_grupo(@Id,CAST(@Datos AS jsonb));", new { Id=id, Datos=datos.ToJsonString() });
}
