using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Npgsql;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class RequiresDatabaseTheoryAttribute : TheoryAttribute
{
    public RequiresDatabaseTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MIRA_TEST_DB")))
        {
            Skip = "Defina MIRA_TEST_DB con la cadena de conexión de una base de pruebas.";
        }
    }
}

/// <summary>Regresión de V1: CRUD con borrado lógico de los seis catálogos contra PostgreSQL real.</summary>
public sealed class CatalogCrudRegressionTests : IDisposable
{
    private const string Marker = "zz-reg-";

    public record Catalog(string Route, string IdKey, string Table, string MarkerColumn, Func<string, object> Create, Func<string, object> Update);

    public static IEnumerable<object[]> Catalogs() =>
    [
        [new Catalog("/api/area_conocimiento", "id", "area_conocimiento", "gran_area",
            m => new { granArea = m, area = "A", disciplina = "D" },
            m => new { granArea = m, area = "A2", disciplina = "D2" })],
        [new Catalog("/api/objetivo_desarrollo_sostenible", "id", "objetivo_desarrollo_sostenible", "nombre",
            m => new { nombre = m, categoria = "Social" },
            m => new { nombre = m, categoria = "Ambiental" })],
        [new Catalog("/api/area_aplicacion", "id", "area_aplicacion", "nombre",
            m => new { nombre = m },
            m => new { nombre = m })],
        [new Catalog("/api/termino_clave", "termino", "termino_clave", "termino",
            m => new { termino = m, terminoIngles = "kw" },
            m => new { termino = m, terminoIngles = "kw-2" })],
        [new Catalog("/api/universidad", "id", "universidad", "nombre",
            m => new { nombre = m, tipo = "Privada", ciudad = "Medellín" },
            m => new { nombre = m, tipo = "Pública", ciudad = "Bogotá" })],
        [new Catalog("/api/linea_investigacion", "id", "linea_investigacion", "nombre",
            m => new { nombre = m, descripcion = "d" },
            m => new { nombre = m, descripcion = "d2" })]
    ];

    private static HttpClient Client(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", TestHostFactory.Token("Administrador"));
        return client;
    }

    [RequiresDatabaseTheory]
    [MemberData(nameof(Catalogs))]
    public async Task FullLifecycle_CreateReadListUpdateLogicalDelete(Catalog catalog)
    {
        using var factory = TestHostFactory.Create();
        using var client = Client(factory);
        var marker = Marker + Guid.NewGuid().ToString("N")[..10];

        var created = await client.PostAsJsonAsync(catalog.Route, catalog.Create(marker));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var key = catalog.IdKey == "id" ? body.GetProperty("id").GetInt32().ToString() : body.GetProperty("termino").GetString()!;
        var item = $"{catalog.Route}/{Uri.EscapeDataString(key)}";

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(item)).StatusCode);
        Assert.Contains(key, await (await client.GetAsync(catalog.Route)).Content.ReadAsStringAsync());

        var updated = await client.PutAsJsonAsync(item, catalog.Update(marker));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync(item)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(item)).StatusCode);
        Assert.DoesNotContain(key, await (await client.GetAsync(catalog.Route)).Content.ReadAsStringAsync());
        Assert.True(await RowStillExistsAsync(catalog, marker), "El borrado debe ser lógico: la fila debe seguir en la base.");
    }

    [RequiresDatabaseTheory]
    [MemberData(nameof(Catalogs))]
    public async Task MissingResource_ReturnsNotFound(Catalog catalog)
    {
        using var factory = TestHostFactory.Create();
        using var client = Client(factory);
        var missing = catalog.IdKey == "id" ? "2147000000" : "zz-no-existe-" + Guid.NewGuid().ToString("N");

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{catalog.Route}/{missing}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{catalog.Route}/{missing}")).StatusCode);
    }

    [RequiresDatabaseTheory]
    [MemberData(nameof(Catalogs))]
    public async Task EmptyPayload_ReturnsBadRequest(Catalog catalog)
    {
        using var factory = TestHostFactory.Create();
        using var client = Client(factory);

        var response = await client.PostAsJsonAsync(catalog.Route, new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [RequiresDatabaseFact]
    public async Task DuplicateKeyword_ReturnsConflict()
    {
        using var factory = TestHostFactory.Create();
        using var client = Client(factory);
        var term = Marker + Guid.NewGuid().ToString("N")[..10];

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/termino_clave", new { termino = term, terminoIngles = "a" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/termino_clave", new { termino = term, terminoIngles = "b" })).StatusCode);
    }

    private static async Task<bool> RowStillExistsAsync(Catalog catalog, string marker)
    {
        await using var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("MIRA_TEST_DB"));
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"SELECT count(*) FROM {catalog.Table} WHERE {catalog.MarkerColumn} = @m AND activo = FALSE", connection);
        command.Parameters.AddWithValue("m", marker);
        return (long)(await command.ExecuteScalarAsync())! > 0;
    }

    public void Dispose()
    {
        var connectionString = Environment.GetEnvironmentVariable("MIRA_TEST_DB");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        foreach (var row in Catalogs())
        {
            var catalog = (Catalog)row[0];
            using var command = new NpgsqlCommand($"DELETE FROM {catalog.Table} WHERE {catalog.MarkerColumn} LIKE @p", connection);
            command.Parameters.AddWithValue("p", Marker + "%");
            command.ExecuteNonQuery();
        }
    }
}
