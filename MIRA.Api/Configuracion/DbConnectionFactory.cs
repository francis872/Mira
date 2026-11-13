using System;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
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
        var rawConfigured = configuration.GetConnectionString("PostgreSql")
            ?? configuration["DATABASE_URL"]
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
            ?? "";

        if (!string.IsNullOrWhiteSpace(rawConfigured) && !rawConfigured.Contains("<TU_"))
        {
            _connectionString = NormalizeConnectionString(rawConfigured);
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

    public static string NormalizeConnectionString(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;
        raw = raw.Trim().Trim('"').Trim('\'');

        // Si viene en formato URL (postgresql://user:pass@host:port/dbname?sslmode=require)
        if (raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(raw);
                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.AbsolutePath.TrimStart('/');
                return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
            }
            catch
            {
                return raw;
            }
        }

        // Si viene en formato Key=Value y es Neon u otro host cloud, garantizar compatibilidad SSL y eliminar Channel Binding
        raw = Regex.Replace(raw, @"Channel Binding\s*=\s*[^;]+;?", "", RegexOptions.IgnoreCase);
        raw = Regex.Replace(raw, @"SSL Mode\s*=\s*VerifyFull;?", "SSL Mode=Require;", RegexOptions.IgnoreCase);

        if (raw.Contains("neon.tech", StringComparison.OrdinalIgnoreCase) || raw.Contains("render.com", StringComparison.OrdinalIgnoreCase))
        {
            if (!raw.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase))
            {
                raw = raw.TrimEnd(';') + ";SSL Mode=Require;";
            }
            if (!raw.Contains("Trust Server Certificate", StringComparison.OrdinalIgnoreCase))
            {
                raw = raw.TrimEnd(';') + ";Trust Server Certificate=true;";
            }
        }

        return raw;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
