using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;
using Npgsql;

namespace MIRA.Api.Repositorios;

public sealed class SedeRepositorio(IDbConnectionFactory connectionFactory) : ISedeRepositorio
{
    public async Task<IReadOnlyList<Sede>> ListarAsync(CancellationToken cancellationToken)
    {
        var sedes = new List<Sede>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT id, nombre, ciudad, activa
            FROM sedes
            WHERE activa = TRUE
            ORDER BY nombre;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            sedes.Add(new Sede
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Ciudad = reader.GetString(2),
                Activa = reader.GetBoolean(3)
            });
        }

        return sedes;
    }

    public async Task<Sede?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT id, nombre, ciudad, activa
            FROM sedes
            WHERE id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Sede
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Ciudad = reader.GetString(2),
            Activa = reader.GetBoolean(3)
        };
    }

    public async Task<Sede> CrearAsync(string nombre, string ciudad, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO sedes (nombre, ciudad, activa)
            VALUES (@nombre, @ciudad, TRUE)
            RETURNING id, nombre, ciudad, activa;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("nombre", nombre);
        command.Parameters.AddWithValue("ciudad", ciudad);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return new Sede
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Ciudad = reader.GetString(2),
            Activa = reader.GetBoolean(3)
        };
    }

    public async Task<Sede?> ActualizarAsync(int id, string nombre, string ciudad, bool activa, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE sedes
            SET nombre = @nombre,
                ciudad = @ciudad,
                activa = @activa
            WHERE id = @id
            RETURNING id, nombre, ciudad, activa;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("nombre", nombre);
        command.Parameters.AddWithValue("ciudad", ciudad);
        command.Parameters.AddWithValue("activa", activa);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Sede
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Ciudad = reader.GetString(2),
            Activa = reader.GetBoolean(3)
        };
    }

    public async Task<bool> DesactivarAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE sedes
            SET activa = FALSE
            WHERE id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }
}
