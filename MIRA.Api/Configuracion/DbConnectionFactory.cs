using System;
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

    public DbConnectionFactory(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("PostgreSql");

        if (!string.IsNullOrWhiteSpace(configured) && !configured.Contains("<TU_"))
        {
            _connectionString = configured;
        }
        else
        {
            // Construir desde variables de entorno (.env) cuando appsettings.json contiene placeholders de seguridad
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5544";
            var db = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "mira_db";
            var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "mira_user";
            var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
            _connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
        }
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
