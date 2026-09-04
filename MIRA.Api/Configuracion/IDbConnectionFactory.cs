using Npgsql;

namespace MIRA.Api.Configuracion;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}
