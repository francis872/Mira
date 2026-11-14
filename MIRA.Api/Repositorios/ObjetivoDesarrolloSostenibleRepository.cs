using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public class ObjetivoDesarrolloSostenibleRepository : IObjetivoDesarrolloSostenibleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ObjetivoDesarrolloSostenibleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ObjetivoDesarrolloSostenible>> GetAllActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_listar_objetivo_desarrollo_sostenible();";
        return await connection.QueryAsync<ObjetivoDesarrolloSostenible>(sql);
    }

    public async Task<ObjetivoDesarrolloSostenible?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_consultar_objetivo_desarrollo_sostenible(@Id);";
        return await connection.QueryFirstOrDefaultAsync<ObjetivoDesarrolloSostenible>(sql, new { Id = id });
    }

    public async Task<ObjetivoDesarrolloSostenible> CreateAsync(ObjetivoDesarrolloSostenible entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_crear_objetivo_desarrollo_sostenible(@Nombre, @Categoria);";
        return await connection.QuerySingleAsync<ObjetivoDesarrolloSostenible>(sql, entity);
    }

    public async Task<bool> UpdateAsync(ObjetivoDesarrolloSostenible entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_actualizar_objetivo_desarrollo_sostenible(@Id, @Nombre, @Categoria);";
        var affected = await connection.QuerySingleAsync<bool>(sql, entity);
        return affected;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_eliminar_objetivo_desarrollo_sostenible(@Id);";
        var affected = await connection.QuerySingleAsync<bool>(sql, new { Id = id });
        return affected;
    }
}

