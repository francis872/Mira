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
}
