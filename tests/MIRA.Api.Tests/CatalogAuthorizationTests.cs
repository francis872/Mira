using System.Net;
using System.Net.Http.Json;
using MIRA.Api.Modelos;
using MIRA.Api.Servicios;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class CatalogAuthorizationTests
{
    public static IEnumerable<object[]> Catalogs =>
    [
        ["/api/area_conocimiento"],
        ["/api/objetivo_desarrollo_sostenible"],
        ["/api/area_aplicacion"],
        ["/api/termino_clave"],
        ["/api/universidad"],
        ["/api/linea_investigacion"]
    ];

    [Theory]
    [MemberData(nameof(Catalogs))]
    public async Task ReadWithoutToken_ReturnsUnauthorized(string route)
    {
        using var factory = TestHostFactory.Create();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(Catalogs))]
    public async Task WriteWithReadOnlyRole_ReturnsForbidden(string route)
    {
        using var factory = TestHostFactory.Create();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", TestHostFactory.Token("Investigador"));

        var post = await client.PostAsJsonAsync(route, new { nombre = "x" });
        var delete = await client.DeleteAsync(route + "/1");

        Assert.Equal(HttpStatusCode.Forbidden, post.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task ReadWithAnyAuthenticatedRole_IsAllowed()
    {
        using var factory = TestHostFactory.Create(services =>
        {
            services.RemoveAll<IUniversidadService>();
            services.AddSingleton<IUniversidadService>(new FakeUniversidadService());
        });
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", TestHostFactory.Token("Investigador"));

        var response = await client.GetAsync("/api/universidad");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WriteWithCoordinatorRole_IsAllowed()
    {
        using var factory = TestHostFactory.Create(services =>
        {
            services.RemoveAll<IUniversidadService>();
            services.AddSingleton<IUniversidadService>(new FakeUniversidadService());
        });
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", TestHostFactory.Token("Coordinador"));

        var response = await client.PostAsJsonAsync("/api/universidad", new { nombre = "U", tipo = "Privada", ciudad = "Medellín" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ExpiredToken_ReturnsUnauthorized()
    {
        using var factory = TestHostFactory.Create();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", TestHostFactory.Token("Administrador", expiredMinutesAgo: 30));

        var response = await client.GetAsync("/api/universidad");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class FakeUniversidadService : IUniversidadService
    {
        public Task<IEnumerable<Universidad>> GetAllActivosAsync() => Task.FromResult<IEnumerable<Universidad>>([]);
        public Task<Universidad?> GetByIdAsync(int id) => Task.FromResult<Universidad?>(null);
        public Task<Universidad> CreateAsync(Universidad entity) { entity.Id = 1; return Task.FromResult(entity); }
        public Task<bool> UpdateAsync(int id, Universidad entity) => Task.FromResult(true);
        public Task<bool> SoftDeleteAsync(int id) => Task.FromResult(true);
    }
}
