using System.Data;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Configuracion;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class ReadinessTests
{
    [RequiresDatabaseFact]
    public async Task ReadinessChecksDatabaseWhileCatalogsRemainProtected()
    {
        using var app=TestHostFactory.Create(authenticated:false);using var client=app.CreateClient();
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/ready")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/health")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/universidad")).StatusCode);
    }

    [Fact]
    public async Task DatabaseFailureIsUnavailableWithoutLeakingConfiguration()
    {
        using var app=TestHostFactory.Create(s=>s.AddSingleton<IDbConnectionFactory>(new FailingFactory()),authenticated:false);
        using var client=app.CreateClient();var response=await client.GetAsync("/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable,response.StatusCode);
        Assert.DoesNotContain("private-configuration",await response.Content.ReadAsStringAsync());
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/health")).StatusCode);
    }

    private sealed class FailingFactory:IDbConnectionFactory
    {
        public IDbConnection CreateConnection()=>throw new InvalidOperationException("private-configuration");
    }
}
