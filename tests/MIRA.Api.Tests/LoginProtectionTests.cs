using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Configuracion;
using MIRA.Api.Repositorios.Seguridad;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class LoginProtectionTests
{
    private const string Key = "isolated-forwarding-test-fixture-32-bytes-never-deploy";
    private static WebApplicationFactory<Program> Factory(bool production = false, ICuentaRepository? repository = null) =>
        TestHostFactory.Create(services => services.AddSingleton<ICuentaRepository>(repository ?? new SeguridadTests.CuentaStub()),
            authenticated:false).WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(production ? "Production" : "Development");
            builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string,string?>
            {
                ["LoginProtection:ForwardingKey"] = Key,
                ["ConnectionStrings:PostgreSql"] = "Host=example.invalid;Database=unused;Username=unused;Password=unused;SSL Mode=VerifyFull;Channel Binding=Require"
            }));
        });

    private static HttpRequestMessage Request(string email, string? origin = null, string? key = Key,
        long? timestamp = null, string? signedBody = null)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new { correo=email, password="Password-test-2026!" });
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        if (origin is not null)
        {
            var stamp = (timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var payload = LoginOriginSettings.Payload(stamp, origin, "POST", "/api/auth/login", Encoding.UTF8.GetBytes(signedBody ?? json));
            var signature = Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(key!), Encoding.UTF8.GetBytes(payload)));
            request.Headers.Add(LoginOriginSettings.Header, $"v1;{stamp};{origin};{signature}");
        }
        return request;
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task IndependentSignedOriginsKeepTheirQuota(bool production)
    {
        using var factory=Factory(production); using var client=factory.CreateClient();
        for(var i=0;i<10;i++)
            Assert.Equal(HttpStatusCode.Unauthorized,(await client.SendAsync(Request($"unknown-{i}@invalid.test","198.51.100.10"))).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests,(await client.SendAsync(Request("another@invalid.test","198.51.100.10"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.SendAsync(Request("test@invalid.test","203.0.113.20"))).StatusCode);
    }

    [Fact]
    public async Task AccountLimitAppliesAcrossDifferentAuthenticatedOriginsAndCasing()
    {
        using var factory=Factory(); using var client=factory.CreateClient();
        for(var i=0;i<10;i++)
            Assert.Equal(HttpStatusCode.Unauthorized,(await client.SendAsync(Request(i%2==0 ? "absent@invalid.test" : "ABSENT@invalid.test", $"192.0.2.{i+1}"))).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests,(await client.SendAsync(Request("absent@invalid.test","192.0.2.99"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.SendAsync(Request("test@invalid.test","192.0.2.100"))).StatusCode);
    }

    [Fact]
    public async Task ArbitraryIpHeadersCannotChangeUnsignedSocketPartition()
    {
        using var factory=Factory(); using var client=factory.CreateClient();
        for(var i=0;i<11;i++)
        {
            using var request=Request($"absent-{i}@invalid.test");
            request.Headers.Add("X-Forwarded-For",$"192.0.2.{i+1}");
            request.Headers.Add("X-Real-IP",$"203.0.113.{i+1}");
            Assert.Equal(i<10 ? HttpStatusCode.Unauthorized : HttpStatusCode.TooManyRequests,(await client.SendAsync(request)).StatusCode);
        }
    }

    [Theory]
    [InlineData("signature")] [InlineData("expired")] [InlineData("future")] [InlineData("body")]
    [InlineData("malformed")] [InlineData("duplicate")]
    public async Task InvalidOriginAssertionsFailBeforeCredentialValidation(string kind)
    {
        using var factory=Factory(); using var client=factory.CreateClient();
        using var request=Request("test@invalid.test","192.0.2.10",
            key:kind=="signature" ? new string('x',64) : Key,
            timestamp:DateTimeOffset.UtcNow.ToUnixTimeSeconds()+(kind=="expired" ? -120 : kind=="future" ? 120 : 0),
            signedBody:kind=="body" ? "{}" : null);
        if(kind=="malformed") { request.Headers.Remove(LoginOriginSettings.Header); request.Headers.Add(LoginOriginSettings.Header,"forged"); }
        if(kind=="duplicate") request.Headers.Add(LoginOriginSettings.Header,"another");
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task UnknownAndKnownIncorrectCredentialsHaveTheSameResponse()
    {
        using var factory=Factory(); using var client=factory.CreateClient();
        using var first=await client.PostAsJsonAsync("/api/auth/login",new{correo="test@invalid.test",password="wrong"});
        using var second=await client.PostAsJsonAsync("/api/auth/login",new{correo="absent@invalid.test",password="wrong"});
        Assert.Equal(HttpStatusCode.Unauthorized,first.StatusCode);
        Assert.Equal(first.StatusCode,second.StatusCode);
        Assert.Equal(await first.Content.ReadAsStringAsync(),await second.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task OversizedLoginIsRejectedWithoutReadingAnUnboundedBody()
    {
        using var factory=Factory(); using var client=factory.CreateClient();
        using var result=await client.PostAsync("/api/auth/login",new StringContent(new string('x',8193),Encoding.UTF8,"application/json"));
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge,result.StatusCode);
    }

    [Fact]
    public async Task NewApplicationInstanceResetsInMemoryQuota()
    {
        using(var factory=Factory())
        using(var client=factory.CreateClient())
        {
            for(var i=0;i<10;i++) await client.SendAsync(Request("absent@invalid.test"));
            Assert.Equal(HttpStatusCode.TooManyRequests,(await client.SendAsync(Request("absent@invalid.test"))).StatusCode);
        }
        using var restarted=Factory(); using var other=restarted.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,(await other.SendAsync(Request("absent@invalid.test"))).StatusCode);
    }

    [Theory]
    [InlineData("")] [InlineData("short")] [InlineData("REPLACE_WITH_AT_LEAST_32_RANDOM_BYTES")]
    public void ProductionRequiresAnIndependentForwardingKey(string key)
    {
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        { ["LoginProtection:ForwardingKey"]=key }).Build();
        using var factory=Factory(production:true);
        var environment=factory.Services.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
        Assert.Throws<InvalidOperationException>(()=>new LoginOriginSettings(config,environment));
    }

    [Fact]
    public void JwtSigningKeyCannotBeReusedForForwarding()
    {
        using var factory=Factory();
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        { ["LoginProtection:ForwardingKey"]=Key, ["Jwt:Key"]=Key }).Build();
        Assert.Throws<InvalidOperationException>(()=>new LoginOriginSettings(config,
            factory.Services.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>()));
    }

    [Fact]
    public async Task DevelopmentWithoutConfiguredKeyAcceptsDirectLoginButNotOriginAssertions()
    {
        using var factory=Factory().WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, c) =>
            c.AddInMemoryCollection(new Dictionary<string,string?> { ["LoginProtection:ForwardingKey"]="" })));
        using var client=factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK,(await client.SendAsync(Request("test@invalid.test"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.SendAsync(Request("test@invalid.test","192.0.2.10"))).StatusCode);
    }

    private sealed class BlockingRepository : ICuentaRepository
    {
        public readonly TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int started;
        public async Task<MIRA.Api.Modelos.Credencial?> Buscar(string correo)
        {
            if(Interlocked.Increment(ref started)==4) Started.TrySetResult();
            await Release.Task;
            return null;
        }
        public Task<MIRA.Api.Modelos.Credencial?> Sesion(string jti,long id) => throw new NotImplementedException();
        public Task RegistrarSesion(string jti,long id,DateTimeOffset expira) => throw new NotImplementedException();
        public Task Revocar(string jti,long id) => throw new NotImplementedException();
        public Task<System.Text.Json.Nodes.JsonNode> Listar() => throw new NotImplementedException();
        public Task<System.Text.Json.Nodes.JsonNode> Roles() => throw new NotImplementedException();
        public Task<System.Text.Json.Nodes.JsonNode> Crear(string correo,string hash,int[] roles) => throw new NotImplementedException();
        public Task<System.Text.Json.Nodes.JsonNode> Editar(long id,string correo,int[] roles) => throw new NotImplementedException();
        public Task<bool> Inactivar(long id) => throw new NotImplementedException();
        public Task Bootstrap(string correo,string hash) => throw new NotImplementedException();
        public Task Restablecer(long id,string hash) => throw new NotImplementedException();
    }

    [Fact]
    public async Task ConcurrentPasswordWorkIsBoundedAcrossOriginsAndAccounts()
    {
        var repository=new BlockingRepository();
        using var factory=Factory(repository:repository);using var client=factory.CreateClient();
        var pending=Enumerable.Range(1,4).Select(i=>client.SendAsync(Request($"absent-{i}@invalid.test",$"192.0.2.{i}"))).ToArray();
        try
        {
            await repository.Started.Task.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.Equal(HttpStatusCode.TooManyRequests,(await client.SendAsync(Request("another@invalid.test","203.0.113.99"))).StatusCode);
        }
        finally { repository.Release.TrySetResult(); }
        var completed=await Task.WhenAll(pending);
        Assert.All(completed,response=>Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode));
    }
}
