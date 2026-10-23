using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Modelos;
using MIRA.Api.Servicios;
using Npgsql;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class ErroresPersistenciaTests
{
    [Theory]
    [InlineData("{\"nombre\":[]}")]
    [InlineData("null")]
    public async Task InvalidJsonContract_ReturnsSpanishValidation(string payload)
    {
        using var factory = TestHostFactory.Create();
        using var client = factory.CreateClient();
        var response = await client.PostAsync("/api/universidad", new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Contains("Revise los datos", body.GetProperty("mensaje").GetString());
        Assert.DoesNotContain("System.", body.ToString());
    }

    private sealed class FailingService(Exception error) : IUniversidadService
    {
        public Task<IEnumerable<Universidad>> GetAllActivosAsync() => Task.FromException<IEnumerable<Universidad>>(error);
        public Task<Universidad?> GetByIdAsync(int id) => throw new NotSupportedException();
        public Task<Universidad> CreateAsync(Universidad entity) => throw new NotSupportedException();
        public Task<bool> UpdateAsync(int id, Universidad entity) => throw new NotSupportedException();
        public Task<bool> SoftDeleteAsync(int id) => throw new NotSupportedException();
    }

    [Theory]
    [InlineData("23503", HttpStatusCode.Conflict)]
    [InlineData("23505", HttpStatusCode.Conflict)]
    [InlineData("23502", HttpStatusCode.BadRequest)]
    [InlineData("22023", HttpStatusCode.BadRequest)]
    [InlineData("22001", HttpStatusCode.BadRequest)]
    [InlineData("XX000", HttpStatusCode.InternalServerError)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task Errors_AreTranslatedWithoutLeakingDetails(string sqlState, HttpStatusCode expected)
    {
        const string secret = "INTERNAL_PASSWORD_AND_SQL";
        Exception error = sqlState == "unexpected" ? new Exception(secret)
            : new PostgresException(secret, "ERROR", "ERROR", sqlState);
        using var factory = TestHostFactory.Create(services =>
            services.AddScoped<IUniversidadService>(_ => new FailingService(error)));
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/universidad");
        Assert.Equal(expected, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("mensaje", body);
        Assert.DoesNotContain(secret, body);
        Assert.DoesNotContain(sqlState, body);
        Assert.DoesNotContain("stackTrace", body);
    }
}
