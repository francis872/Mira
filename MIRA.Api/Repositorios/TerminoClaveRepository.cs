using Dapper;
using Npgsql;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public class TerminoClaveRepository : ITerminoClaveRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TerminoClaveRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<TerminoClave>> GetAllActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_listar_termino_clave();";
        return await connection.QueryAsync<TerminoClave>(sql);
    }

    public async Task<TerminoClave?> GetByTerminoAsync(string termino)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_consultar_termino_clave(@Termino);";
        return await connection.QueryFirstOrDefaultAsync<TerminoClave>(sql, new { Termino = termino });
    }

    public async Task<TerminoClave> CreateAsync(TerminoClave entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM public.sp_crear_termino_clave(@Termino, @TerminoIngles);";
        try
        {
            return await connection.QuerySingleAsync<TerminoClave>(sql, entity);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Incluye claves inactivas y colisiones entre solicitudes concurrentes.
            throw new InvalidOperationException("El término clave ya existe, activo o inactivo.");
        }
    }

    public async Task<bool> UpdateAsync(string termino, TerminoClave entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_actualizar_termino_clave(@Termino, @TerminoIngles);";
        var affected = await connection.QuerySingleAsync<bool>(sql, new { entity.TerminoIngles, Termino = termino });
        return affected;
    }

    public async Task<bool> SoftDeleteAsync(string termino)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT public.sp_eliminar_termino_clave(@Termino);";
        var affected = await connection.QuerySingleAsync<bool>(sql, new { Termino = termino });
        return affected;
    }
}

