using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;
using Npgsql;

namespace MIRA.Api.Repositorios;

public sealed class UsuarioRepositorio(IDbConnectionFactory connectionFactory) : IUsuarioRepositorio
{
    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT u.id, u.nombre, u.correo, u.password_hash,
                   COALESCE(array_agg(r.nombre) FILTER (WHERE r.nombre IS NOT NULL), ARRAY['Docente']) AS roles
            FROM usuarios u
            LEFT JOIN usuario_roles ur ON ur.usuario_id = u.id
            LEFT JOIN roles r ON r.id = ur.rol_id
            WHERE u.correo = @correo
            GROUP BY u.id, u.nombre, u.correo, u.password_hash
            LIMIT 1;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("correo", correo);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Usuario
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            Correo = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            Roles = reader.GetFieldValue<string[]>(4)
        };
    }

    public async Task<IReadOnlyList<string>> ObtenerRolesAsync(int usuarioId, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT r.nombre
            FROM usuario_roles ur
            JOIN roles r ON r.id = ur.rol_id
            WHERE ur.usuario_id = @usuarioId
            ORDER BY r.nombre;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("usuarioId", usuarioId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var roles = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            roles.Add(reader.GetString(0));
        }

        return roles;
    }

    public async Task<bool> AsignarRolAsync(int usuarioId, string rol, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO usuario_roles (usuario_id, rol_id)
            SELECT @usuarioId, r.id
            FROM roles r
            WHERE r.nombre = @rol
            ON CONFLICT DO NOTHING;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("usuarioId", usuarioId);
        command.Parameters.AddWithValue("rol", rol);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> RemoverRolAsync(int usuarioId, string rol, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            DELETE FROM usuario_roles ur
            USING roles r
            WHERE ur.rol_id = r.id
              AND ur.usuario_id = @usuarioId
              AND r.nombre = @rol;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("usuarioId", usuarioId);
        command.Parameters.AddWithValue("rol", rol);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }
}
