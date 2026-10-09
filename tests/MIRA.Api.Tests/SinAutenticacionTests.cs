using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Modelos;
using MIRA.Api.Servicios;
using Xunit;

namespace MIRA.Api.Tests;

/// <summary>La entrega académica V1/V2 no incluye inicio de sesión: la API no exige tokens ni expone rutas de autenticación.</summary>
public sealed class SinAutenticacionTests
{
    private sealed class UniversidadStub : IUniversidadService
    {
        public Task<IEnumerable<Universidad>> GetAllActivosAsync() =>
            Task.FromResult<IEnumerable<Universidad>>([new Universidad { Id = 1, Nombre = "Stub", Tipo = "Privada", Ciudad = "Medellín" }]);
        public Task<Universidad?> GetByIdAsync(int id) => Task.FromResult<Universidad?>(null);
        public Task<Universidad> CreateAsync(Universidad entity) { entity.Id = 7; return Task.FromResult(entity); }
        public Task<bool> UpdateAsync(int id, Universidad entity) => Task.FromResult(true);
        public Task<bool> SoftDeleteAsync(int id) => Task.FromResult(true);
    }

    private static HttpClient AnonymousClient(out Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory)
    {
        factory = TestHostFactory.Create(services =>
        {
            services.AddScoped<IUniversidadService, UniversidadStub>();
        });
        return factory.CreateClient();
    }

    [Fact]
    public async Task Health_AnswersWithoutCredentials()
    {
        using var client = AnonymousClient(out var factory);
        using var _ = factory;
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task CatalogReads_DoNotRequireToken()
    {
        using var client = AnonymousClient(out var factory);
        using var _ = factory;
        var response = await client.GetAsync("/api/universidad");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Stub", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task CatalogWrites_DoNotRequireToken()
    {
        using var client = AnonymousClient(out var factory);
        using var _ = factory;
        var response = await client.PostAsJsonAsync("/api/universidad", new { nombre = "Nueva", tipo = "Privada", ciudad = "Cali" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync("/api/universidad/7")).StatusCode);
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/usuarios")]
    public async Task AuthenticationRoutesAreNotExposed(string route)
    {
        using var client = AnonymousClient(out var factory);
        using var _ = factory;
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync(route, new { })).StatusCode);
    }
}
