using MIRA.Api.Configuracion;
using MIRA.Api.Excepciones;
using MIRA.Api.Modelos;
using Npgsql;

namespace MIRA.Api.Repositorios;

public sealed class CatalogoRepositorio(IDbConnectionFactory connectionFactory) : ICatalogoRepositorio
{
    public async Task<IReadOnlyList<CatalogoItem>> ListarAsync(string tipo, CancellationToken cancellationToken)
    {
        var tabla = ResolverTabla(tipo);
        var sql = $"SELECT id, nombre, activo FROM {tabla} ORDER BY nombre;";

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
        var tabla = ResolverTabla(tipo);
        var sql = $"INSERT INTO {tabla} (nombre, activo) VALUES (@nombre, TRUE) RETURNING id, nombre, activo;";

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

    private static string ResolverTabla(string tipo) => tipo switch
    {
        "areas-conocimiento" => "areas_conocimiento",
        "ods" => "ods",
        "aplicaciones" => "areas_aplicacion",
        "palabras-clave" => "palabras_clave",
        _ => throw new ApiException("Tipo de catálogo inválido.", StatusCodes.Status404NotFound)
    };
}
