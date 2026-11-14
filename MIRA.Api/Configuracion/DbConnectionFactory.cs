using System.Data;
using Npgsql;

namespace MIRA.Api.Configuracion;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration, IHostEnvironment environment)
    {
        _connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'PostgreSql' en la configuración.");
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Configure ConnectionStrings__PostgreSql mediante un secreto externo.");
        var settings = new NpgsqlConnectionStringBuilder(_connectionString);
        if (environment.IsProduction() &&
            (settings.SslMode != SslMode.VerifyFull || settings.ChannelBinding != ChannelBinding.Require || settings.IncludeErrorDetail))
            throw new InvalidOperationException("Producción requiere SSL Mode=VerifyFull, Channel Binding=Require e Include Error Detail=false.");
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}

