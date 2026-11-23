using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios.Seguridad;
using MIRA.Api.Servicios;
using MIRA.Api.Servicios.Seguridad;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class SeguridadTests
{
    internal sealed class CuentaStub : ICuentaRepository
    {
        private static readonly string Hash = new PasswordService().Hash("Password-test-2026!");
        public bool Activo = true;
        public string[] RolesActuales = ["Investigador"];
        public HashSet<string> Sessions = [];
        public Task<Credencial?> Buscar(string correo) => Task.FromResult<Credencial?>(correo == "test@invalid.test" ? new(1, correo, Hash, Activo, RolesActuales) : null);
        public Task<Credencial?> Sesion(string jti, long id) => Task.FromResult<Credencial?>(Sessions.Contains(jti) && id == 1 ? new(1, "test@invalid.test", Hash, Activo, RolesActuales) : null);
        public Task RegistrarSesion(string jti, long id, DateTimeOffset expira) { Sessions.Add(jti); return Task.CompletedTask; }
        public Task Revocar(string jti, long id) { Sessions.Remove(jti); return Task.CompletedTask; }
        public Task<JsonNode> Listar() => Task.FromResult<JsonNode>(new JsonArray());
        public Task<JsonNode> Roles() => Task.FromResult<JsonNode>(new JsonArray());
        public Task<JsonNode> Crear(string correo, string hash, int[] roles) => throw new NotImplementedException();
        public Task<JsonNode> Editar(long id, string correo, int[] roles) => throw new NotImplementedException();
        public Task<bool> Inactivar(long id) => throw new NotImplementedException();
        public Task Bootstrap(string correo, string hash) => throw new NotImplementedException();
        public Task Restablecer(long id, string hash) => throw new NotImplementedException();
    }
    private sealed class UniversidadStub : IUniversidadService
    {
        public Task<IEnumerable<Universidad>> GetAllActivosAsync() => Task.FromResult<IEnumerable<Universidad>>([]);
        public Task<Universidad?> GetByIdAsync(int id) => Task.FromResult<Universidad?>(null);
        public Task<Universidad> CreateAsync(Universidad entity) { entity.Id=7; return Task.FromResult(entity); }
        public Task<bool> UpdateAsync(int id, Universidad entity) => Task.FromResult(true);
        public Task<bool> SoftDeleteAsync(int id) => Task.FromResult(true);
    }
    private static Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> Factory(CuentaStub repository) => TestHostFactory.Create(services =>
    {
        services.AddSingleton<ICuentaRepository>(repository);
        services.AddScoped<IUniversidadService, UniversidadStub>();
    }, authenticated:false);
    private static async Task<string> Login(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { correo=" TEST@invalid.test ".Trim(), password="Password-test-2026!" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.DoesNotContain("password", body.ToJsonString(), StringComparison.OrdinalIgnoreCase);
        var token = body["token"]!.GetValue<string>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return token;
    }
    [Fact]
    public async Task Login_Read_ForbiddenWrite_Logout_Revoked()
    {
        using var factory=Factory(new()); using var client=factory.CreateClient();
        var token=await Login(client);
        var jwt=new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.All(new[]{"sub","email","role","iat","exp","iss","aud","jti"}, name => Assert.Contains(jwt.Claims,c => c.Type==name));
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/api/universidad")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync("/api/universidad",new {nombre="Prueba",tipo="Privada",ciudad="Bogotá"})).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/auth/usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync("/api/auth/logout",new {})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/universidad")).StatusCode);
    }
    [Theory]
    [InlineData("test@invalid.test", "incorrecta")]
    [InlineData("unknown@invalid.test", "Password-test-2026!")]
    public async Task BadLoginIs401(string email, string password)
    {
        using var factory=Factory(new()); using var client=factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/auth/login",new {correo=email,password})).StatusCode);
    }
    [Theory]
    [InlineData(null)] [InlineData("invalid-token")]
    public async Task MissingOrInvalidTokenIs401(string? token)
    {
        using var factory=Factory(new()); using var client=factory.CreateClient();
        if(token is not null) client.DefaultRequestHeaders.Authorization=new("Bearer",token);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/universidad")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/health")).StatusCode);
    }
    [Theory]
    [InlineData("expired")] [InlineData("signature")] [InlineData("issuer")] [InlineData("audience")]
    public async Task InvalidJwtClaimsAre401(string kind)
    {
        var repository=new CuentaStub(); using var factory=Factory(repository); using var client=factory.CreateClient();
        var token=await Login(client); var original=new JwtSecurityTokenHandler().ReadJwtToken(token);
        var settings=factory.Services.GetRequiredService<JwtSettings>();
        var jwt=new JwtSecurityToken(kind=="issuer" ? "wrong" : settings.Issuer,kind=="audience" ? "wrong" : settings.Audience,
            original.Claims.Where(c => c.Type is not ("exp" or "nbf" or "iss" or "aud")),
            DateTime.UtcNow.AddMinutes(-10),kind=="expired" ? DateTime.UtcNow.AddSeconds(-1) : DateTime.UtcNow.AddMinutes(10),
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(kind=="signature" ? new string('x',64) : settings.Key)),SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization=new("Bearer",new JwtSecurityTokenHandler().WriteToken(jwt));
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/auth/me")).StatusCode);
    }
    [Theory]
    [InlineData("Investigador",403,403)] [InlineData("Coordinador",201,403)] [InlineData("Administrador",201,200)]
    public async Task RoleMatrix(string role,int create,int delete)
    {
        using var factory=Factory(new(){RolesActuales=[role]}); using var client=factory.CreateClient(); await Login(client);
        Assert.Equal(create,(int)(await client.PostAsJsonAsync("/api/universidad",new {nombre="Prueba",tipo="Privada",ciudad="Bogotá"})).StatusCode);
        Assert.Equal(delete,(int)(await client.DeleteAsync("/api/universidad/7")).StatusCode);
    }
    [Fact]
    public async Task InactiveAccountAndRoleChangesInvalidateSession()
    {
        var repository=new CuentaStub(); using var factory=Factory(repository); using var client=factory.CreateClient(); await Login(client);
        repository.Activo=false;
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/auth/login",new {correo="test@invalid.test",password="Password-test-2026!"})).StatusCode);
        repository.Activo=true; await Login(client); repository.RolesActuales=["Administrador"];
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/auth/me")).StatusCode);
    }
    [Fact]
    public void BcryptUsesSaltAndRejectsPlaintextAndWrongPassword()
    {
        var service=new PasswordService(); var password="Password-test-2026!"; var hash=service.Hash(password);
        Assert.StartsWith("$2a$12$",hash); Assert.NotEqual(password,hash); Assert.NotEqual(hash,service.Hash(password));
        Assert.True(service.Verify(password,hash)); Assert.False(service.Verify("wrong",hash)); Assert.False(service.Verify(password,password));
        Assert.Throws<SeguridadValidationException>(() => service.Hash(new string('ñ',40)));
    }
    [Fact]
    public async Task LoginRateLimitReturns429()
    {
        using var factory=Factory(new()); using var client=factory.CreateClient();
        for (var i=0;i<10;i++)
            Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/auth/login",new {})).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests,(await client.PostAsJsonAsync("/api/auth/login",new {})).StatusCode);
    }
    [Fact]
    public async Task SwaggerDocumentsBearerAndAnonymousLogin()
    {
        using var factory=Factory(new()).WithWebHostBuilder(builder=>builder.UseEnvironment("Development"));
        using var client=factory.CreateClient();
        var response=await client.GetAsync("/swagger/v2/swagger.json");
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var document=(await response.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.Equal("bearer",document["components"]!["securitySchemes"]!["Bearer"]!["scheme"]!.GetValue<string>());
        Assert.Empty(document["paths"]!["/api/auth/login"]!["post"]!["security"]!.AsArray());
    }
    [Fact]
    public async Task UnknownRoleCannotLogin()
    {
        using var factory=Factory(new(){RolesActuales=["Desconocido"]});using var client=factory.CreateClient();
        var response=await client.PostAsJsonAsync("/api/auth/login",new {correo="test@invalid.test",password="Password-test-2026!"});
        Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);
    }
    [Theory]
    [InlineData("Investigador")]
    [InlineData("Coordinador")]
    public async Task EveryAdministrativeActionRequiresAdministrator(string role)
    {
        using var factory=Factory(new(){RolesActuales=[role]});using var client=factory.CreateClient();await Login(client);
        foreach(var (method,path) in new[]{("GET","roles"),("GET","usuarios"),("POST","usuarios"),("PUT","usuarios/1"),("DELETE","usuarios/1"),("PUT","usuarios/1/password")})
        {
            using var request=new HttpRequestMessage(new HttpMethod(method),"/api/auth/"+path);
            if(method is "POST" or "PUT")request.Content=JsonContent.Create(new {});
            Assert.Equal(HttpStatusCode.Forbidden,(await client.SendAsync(request)).StatusCode);
        }
    }
    [Theory]
    [InlineData(11,false)]
    [InlineData(12,true)]
    [InlineData(72,true)]
    [InlineData(73,false)]
    public void PasswordBoundariesAreEnforced(int length,bool accepted)
    {
        var service=new PasswordService();var password=new string('x',length);
        if(accepted)Assert.True(service.Verify(password,service.Hash(password)));
        else Assert.Throws<SeguridadValidationException>(()=>service.Hash(password));
    }
}
