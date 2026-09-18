using MIRA.Api.Configuracion;
using MIRA.Api.Excepciones;
using MIRA.Api.Modelos;
using Npgsql;

namespace MIRA.Api.Repositorios;

public sealed class CatalogoRepositorio(IDbConnectionFactory connectionFactory) : ICatalogoRepositorio
{
    public async Task<IReadOnlyList<CatalogoItem>> ListarAsync(string tipo, CancellationToken cancellationToken)
    {
        var sql = SqlListar(tipo);

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<CatalogoItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CatalogoItem
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Activo = reader.GetBoolean(2)
            });
        }

        return items;
    }

    public async Task<CatalogoItem> CrearAsync(string tipo, string nombre, CancellationToken cancellationToken)
    {
        var sql = SqlCrear(tipo);

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("nombre", nombre);

        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);

            return new CatalogoItem
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Activo = reader.GetBoolean(2)
            };
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new ApiException("El item ya existe en el catálogo.", StatusCodes.Status409Conflict);
        }
    }

    public async Task<CatalogoItem?> ObtenerPorIdAsync(string tipo, int id, CancellationToken cancellationToken)
    {
        var sql = SqlObtener(tipo);

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? Mapear(reader) : null;
    }

    public async Task<CatalogoItem?> ActualizarAsync(string tipo, int id, string nombre, bool activo, CancellationToken cancellationToken)
    {
        var sql = SqlActualizar(tipo);

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("nombre", nombre);
        command.Parameters.AddWithValue("activo", activo);

        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Mapear(reader) : null;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            throw new ApiException("El item ya existe en el catálogo.", StatusCodes.Status409Conflict);
        }
    }

    public async Task<bool> DesactivarAsync(string tipo, int id, CancellationToken cancellationToken)
    {
        var sql = SqlDesactivar(tipo);

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static CatalogoItem Mapear(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Nombre = reader.GetString(1),
        Activo = reader.GetBoolean(2)
    };

    private static string SqlListar(string tipo) => tipo switch
    {
        "areas-conocimiento" => "SELECT id, nombre, activo FROM areas_conocimiento WHERE activo = TRUE ORDER BY nombre;",
        "ods" => "SELECT id, nombre, activo FROM ods WHERE activo = TRUE ORDER BY nombre;",
        "aplicaciones" => "SELECT id, nombre, activo FROM areas_aplicacion WHERE activo = TRUE ORDER BY nombre;",
        "palabras-clave" => "SELECT id, nombre, activo FROM palabras_clave WHERE activo = TRUE ORDER BY nombre;",
        _ => throw new ApiException("Tipo de catálogo inválido.", StatusCodes.Status404NotFound)
    };

    private static string SqlCrear(string tipo) => tipo switch
    {
        "areas-conocimiento" => "INSERT INTO areas_conocimiento (nombre, activo) VALUES (@nombre, TRUE) RETURNING id, nombre, activo;",
        "ods" => "INSERT INTO ods (nombre, activo) VALUES (@nombre, TRUE) RETURNING id, nombre, activo;",
        "aplicaciones" => "INSERT INTO areas_aplicacion (nombre, activo) VALUES (@nombre, TRUE) RETURNING id, nombre, activo;",
        "palabras-clave" => "INSERT INTO palabras_clave (nombre, activo) VALUES (@nombre, TRUE) RETURNING id, nombre, activo;",
        _ => throw new ApiException("Tipo de catálogo inválido.", StatusCodes.Status404NotFound)
    };

    private static string SqlObtener(string tipo) => tipo switch
    {
        "areas-conocimiento" => "SELECT id, nombre, activo FROM areas_conocimiento WHERE id = @id;",
        "ods" => "SELECT id, nombre, activo FROM ods WHERE id = @id;",
        "aplicaciones" => "SELECT id, nombre, activo FROM areas_aplicacion WHERE id = @id;",
        "palabras-clave" => "SELECT id, nombre, activo FROM palabras_clave WHERE id = @id;",
        _ => throw new ApiException("Tipo de catálogo inválido.", StatusCodes.Status404NotFound)
    };

    private static string SqlActualizar(string tipo) => tipo switch
    {
        "areas-conocimiento" => "UPDATE areas_conocimiento SET nombre = @nombre, activo = @activo WHERE id = @id RETURNING id, nombre, activo;",
        "ods" => "UPDATE ods SET nombre = @nombre, activo = @activo WHERE id = @id RETURNING id, nombre, activo;",
        "aplicaciones" => "UPDATE areas_aplicacion SET nombre = @nombre, activo = @activo WHERE id = @id RETURNING id, nombre, activo;",
        "palabras-clave" => "UPDATE palabras_clave SET nombre = @nombre, activo = @activo WHERE id = @id RETURNING id, nombre, activo;",
        _ => throw new ApiException("Tipo de catálogo inválido.", StatusCodes.Status404NotFound)
    };

    private static string SqlDesactivar(string tipo) => tipo switch
    {
        "areas-conocimiento" => "UPDATE areas_conocimiento SET activo = FALSE WHERE id = @id AND activo = TRUE;",
        "ods" => "UPDATE ods SET activo = FALSE WHERE id = @id AND activo = TRUE;",
        "aplicaciones" => "UPDATE areas_aplicacion SET activo = FALSE WHERE id = @id AND activo = TRUE;",
        "palabras-clave" => "UPDATE palabras_clave SET activo = FALSE WHERE id = @id AND activo = TRUE;",
        _ => throw new ApiException("Tipo de catálogo inválido.", StatusCodes.Status404NotFound)
    };
}
