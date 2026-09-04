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
            SELECT id, nombre, correo, password_hash, rol
            FROM usuarios
            WHERE correo = @correo
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
            Rol = reader.GetString(4)
        };
    }
}
