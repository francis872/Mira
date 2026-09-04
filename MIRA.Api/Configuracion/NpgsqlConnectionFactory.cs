using Npgsql;

namespace MIRA.Api.Configuracion;

public sealed class NpgsqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString =
        configuration.GetConnectionString("PostgreSql")
        ?? throw new InvalidOperationException("ConnectionStrings:PostgreSql is required.");

    public NpgsqlConnection CreateConnection() => new(_connectionString);
}
