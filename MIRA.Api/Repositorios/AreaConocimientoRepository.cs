using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public class AreaConocimientoRepository : IAreaConocimientoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AreaConocimientoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<AreaConocimiento>> GetAllActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_listar_area_conocimiento();";
        return await connection.QueryAsync<AreaConocimiento>(sql);
    }

    public async Task<AreaConocimiento?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_consultar_area_conocimiento(@Id);";
        return await connection.QueryFirstOrDefaultAsync<AreaConocimiento>(sql, new { Id = id });
    }

    public async Task<AreaConocimiento> CreateAsync(AreaConocimiento entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_crear_area_conocimiento(@GranArea, @Area, @Disciplina);";
        return await connection.QuerySingleAsync<AreaConocimiento>(sql, entity);
    }

    public async Task<bool> UpdateAsync(AreaConocimiento entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_actualizar_area_conocimiento(@Id, @GranArea, @Area, @Disciplina);";
        var affected = await connection.QuerySingleAsync<bool>(sql, entity);
        return affected;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_eliminar_area_conocimiento(@Id);";
        var affected = await connection.QuerySingleAsync<bool>(sql, new { Id = id });
        return affected;
    }
}

