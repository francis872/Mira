using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public class UniversidadRepository : IUniversidadRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UniversidadRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Universidad>> GetAllActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_listar_universidad();";
        return await connection.QueryAsync<Universidad>(sql);
    }

    public async Task<Universidad?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_consultar_universidad(@Id);";
        return await connection.QueryFirstOrDefaultAsync<Universidad>(sql, new { Id = id });
    }

    public async Task<Universidad> CreateAsync(Universidad entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_crear_universidad(@Nombre, @Tipo, @Ciudad);";
        return await connection.QuerySingleAsync<Universidad>(sql, entity);
    }

    public async Task<bool> UpdateAsync(Universidad entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_actualizar_universidad(@Id, @Nombre, @Tipo, @Ciudad);";
        var affected = await connection.QuerySingleAsync<bool>(sql, entity);
        return affected;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_eliminar_universidad(@Id);";
        var affected = await connection.QuerySingleAsync<bool>(sql, new { Id = id });
        return affected;
    }
}

