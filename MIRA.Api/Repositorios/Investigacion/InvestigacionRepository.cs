using System.Text.Json.Nodes;
using System.Data.Common;
using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios.Investigacion;

public sealed partial class InvestigacionRepository(IDbConnectionFactory factory)
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
    public async Task<JsonNode?> Actualizar(RecursoInvestigacion recurso, int id, JsonObject datos, bool puedeRetirar)
    {
        var name=Nombre(recurso);
        if(name!="lineas_investigacion" || puedeRetirar)
            return await Consultar($"SELECT public.sp_actualizar_v2_{name}(@Id,CAST(@Datos AS jsonb));",new {Id=id,Datos=datos.ToJsonString()});
        using var connection=factory.CreateConnection();
        if(connection is DbConnection db) await db.OpenAsync(); else connection.Open();
        using var transaction=connection.BeginTransaction();
        var parent=await connection.QuerySingleOrDefaultAsync<int?>("SELECT id FROM public.lineas_investigacion WHERE id=@Id AND activo FOR UPDATE",new {Id=id},transaction);
        if(parent is null) return null;
        foreach(var (table,key,field) in new[]{("linea_disciplinas","disciplina_id","disciplina_ids"),("linea_ods","ods_id","ods_ids")})
        {
            var current=await connection.QueryAsync<int>($"SELECT {key} FROM public.{table} WHERE linea_id=@Id",new {Id=id},transaction);
            var kept=datos[field]!.AsArray().Select(n=>n!.GetValue<int>()).ToHashSet();
            if(current.Any(link=>!kept.Contains(link)))
                throw new MIRA.Api.Servicios.OperacionInvestigacionNoAutorizadaException("Solo un Administrador puede retirar asociaciones existentes. Conserve las disciplinas y ODS registrados para guardar otros cambios.");
        }
        var json=await connection.QuerySingleOrDefaultAsync<string>("SELECT public.sp_actualizar_v2_lineas_investigacion(@Id,CAST(@Datos AS jsonb));",new {Id=id,Datos=datos.ToJsonString()},transaction);
        transaction.Commit();
        return json is null ? null : JsonNode.Parse(json);
    }
    public async Task<bool> Eliminar(RecursoInvestigacion recurso, int id, int? derecha = null)
    {
        using var connection = factory.CreateConnection();
        var argumentos = recurso.EsPuente ? "@Id,@Derecha" : "@Id";
        return await connection.QuerySingleAsync<bool>($"SELECT public.sp_eliminar_v2_{Nombre(recurso)}({argumentos});", new { Id=id, Derecha=derecha });
    }
    public Task<JsonNode?> Usuarios() => Consultar("SELECT public.sp_referencias_v2_usuarios();");
    public Task<JsonNode?> Ficha(int id) => Consultar("SELECT public.sp_consultar_v2_ficha_grupo(@Id);", new { Id=id });
    public async Task<JsonNode?> GuardarFicha(int? id, JsonObject datos, bool puedeRetirar)
    {
        using var connection = factory.CreateConnection();
        if (connection is DbConnection db) await db.OpenAsync(); else connection.Open();
        using var transaction = connection.BeginTransaction();
        if (id is not null && !puedeRetirar)
        {
            // El bloqueo del padre se comparte con el trigger de altas de semilleros.
            // La autorización y la escritura usan la misma conexión/transacción.
            var parent = await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT id FROM public.grupos_investigacion WHERE id=@Id AND activo FOR UPDATE", new { Id=id }, transaction);
            if (parent is null) return null;
            var current = await connection.QueryAsync<int>("SELECT id FROM public.semilleros WHERE grupo_id=@Id AND activo", new { Id=id }, transaction);
            var kept = datos["semilleros"]!.AsArray().OfType<JsonObject>().Where(row => row["id"] is not null).Select(row => row["id"]!.GetValue<int>()).ToHashSet();
            if (current.Any(child => !kept.Contains(child)))
                throw new MIRA.Api.Servicios.OperacionInvestigacionNoAutorizadaException("Solo un Administrador puede retirar semilleros existentes. Conserve sus renglones para guardar otros cambios.");
        }
        var json = await connection.QuerySingleOrDefaultAsync<string>(
            "SELECT public.sp_guardar_v2_ficha_grupo(@Id,CAST(@Datos AS jsonb));", new { Id=id, Datos=datos.ToJsonString() }, transaction);
        transaction.Commit();
        return json is null ? null : JsonNode.Parse(json);
    }
    public Task<JsonNode?> AsignarLineas(string recurso,int padre,int[] lineas) =>
        Consultar("SELECT public.sp_asignar_v2_lineas(@Recurso,@Padre,CAST(@Lineas AS jsonb));",new {Recurso=recurso,Padre=padre,Lineas=System.Text.Json.JsonSerializer.Serialize(lineas)});
    public Task<JsonNode?> ResumenLineas(string recurso,int padre) =>
        Consultar("SELECT public.sp_resumen_v2_lineas(@Recurso,@Padre);",new {Recurso=recurso,Padre=padre});
    public Task<JsonNode?> FiltrarSemilleros(int? grupo,int? docente) =>
        Consultar("SELECT public.sp_filtrar_v2_semilleros(@Grupo,@Docente);",new {Grupo=grupo,Docente=docente});
    public Task<JsonNode?> OpcionesReportes() => Consultar("SELECT public.sp_opciones_v2_reportes();");
    public Task<JsonNode?> LineasInactivas() => Consultar("SELECT public.sp_listar_v2_lineas_inactivas();");
    public Task<JsonNode?> EstadoLinea(int id,bool activo) => Consultar("SELECT public.sp_estado_v2_linea(@Id,@Activo);",new {Id=id,Activo=activo});
    public Task<JsonNode?> ReporteIntegrantes(int grupo) =>
        Consultar("SELECT public.sp_reporte_v2_integrantes(@Grupo);",new {Grupo=grupo});
    public Task<JsonNode?> ReporteArticulacion(int? sede,int? linea) =>
        Consultar("SELECT public.sp_reporte_v2_articulacion(@Sede,@Linea);",new {Sede=sede,Linea=linea});
}
