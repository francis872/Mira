using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public class AreaAplicacionRepository : IAreaAplicacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AreaAplicacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<AreaAplicacion>> GetAllActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_listar_area_aplicacion();";
        return await connection.QueryAsync<AreaAplicacion>(sql);
    }

    public async Task<AreaAplicacion?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_consultar_area_aplicacion(@Id);";
        return await connection.QueryFirstOrDefaultAsync<AreaAplicacion>(sql, new { Id = id });
    }

    public async Task<AreaAplicacion> CreateAsync(AreaAplicacion entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_crear_area_aplicacion(@Nombre);";
        return await connection.QuerySingleAsync<AreaAplicacion>(sql, entity);
    }

    public async Task<bool> UpdateAsync(AreaAplicacion entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_actualizar_area_aplicacion(@Id, @Nombre);";
        var affected = await connection.QuerySingleAsync<bool>(sql, entity);
        return affected;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_eliminar_area_aplicacion(@Id);";
        var affected = await connection.QuerySingleAsync<bool>(sql, new { Id = id });
        return affected;
    }
}

