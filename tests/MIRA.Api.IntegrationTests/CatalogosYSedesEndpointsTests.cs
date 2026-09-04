using System.Net;
using System.Net.Http.Json;

namespace MIRA.Api.IntegrationTests;

public class CatalogosYSedesEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task CatalogosAndSedes_Flow_WorksWithoutLogin()
    {
        var client = factory.CreateClient();

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

}
