using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public class LineaInvestigacionRepository : ILineaInvestigacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LineaInvestigacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<LineaInvestigacion>> GetAllActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_listar_linea_investigacion();";
        return await connection.QueryAsync<LineaInvestigacion>(sql);
    }

    public async Task<LineaInvestigacion?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_consultar_linea_investigacion(@Id);";
        return await connection.QueryFirstOrDefaultAsync<LineaInvestigacion>(sql, new { Id = id });
    }

    public async Task<LineaInvestigacion> CreateAsync(LineaInvestigacion entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_crear_linea_investigacion(@Nombre, @Descripcion);";
        return await connection.QuerySingleAsync<LineaInvestigacion>(sql, entity);
    }

    public async Task<bool> UpdateAsync(LineaInvestigacion entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_actualizar_linea_investigacion(@Id, @Nombre, @Descripcion);";
        var affected = await connection.QuerySingleAsync<bool>(sql, entity);
        return affected;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_eliminar_linea_investigacion(@Id);";
        var affected = await connection.QuerySingleAsync<bool>(sql, new { Id = id });
        return affected;
    }
}

