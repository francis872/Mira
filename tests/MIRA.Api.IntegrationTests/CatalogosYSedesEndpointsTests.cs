using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MIRA.Api.IntegrationTests;

public class CatalogosYSedesEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task CatalogosAndSedes_Flow_WorksWithAdminToken()
    {
        var client = factory.CreateClient();
        var token = await LoginAndGetToken(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var crearCatalogo = await client.PostAsJsonAsync("/api/catalogos/ods", new { nombre = "ODS 7" });
        Assert.Equal(HttpStatusCode.Created, crearCatalogo.StatusCode);

        var listarCatalogo = await client.GetAsync("/api/catalogos/ods");
        listarCatalogo.EnsureSuccessStatusCode();

        var crearSede = await client.PostAsJsonAsync("/api/sedes", new { nombre = "Sede Bogota", ciudad = "Bogota" });
        Assert.Equal(HttpStatusCode.Created, crearSede.StatusCode);

        var updateSede = await client.PutAsJsonAsync("/api/sedes/1", new { nombre = "Sede Medellin Centro", ciudad = "Medellin", activa = true });
        updateSede.EnsureSuccessStatusCode();

        var listSedes = await client.GetAsync("/api/sedes");
        listSedes.EnsureSuccessStatusCode();

        var disable = await client.DeleteAsync("/api/sedes/1");
        Assert.Equal(HttpStatusCode.NoContent, disable.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoints_WithoutToken_ReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/sedes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<string> LoginAndGetToken(HttpClient client)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            correo = "admin@mira.local",
            password = "Admin123!"
        });

        loginResponse.EnsureSuccessStatusCode();
        var json = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString() ?? string.Empty;
    }
}
