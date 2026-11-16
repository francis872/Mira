using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios.Seguridad;
using MIRA.Api.Servicios.Seguridad;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class RegistroTests
{
    private const string Password = "Only-for-isolated-tests!";
    private sealed class RegistroStore : ICuentaRepository
    {
        public Credencial? User;
        public int[]? CreatedRoles;
        public bool RoleAvailable = true;
        private readonly HashSet<string> sessions = [];
        public Task<Credencial?> Buscar(string correo) => Task.FromResult(User?.Correo == correo ? User : null);
        public Task<JsonNode> Roles() => Task.FromResult<JsonNode>(RoleAvailable ? JsonNode.Parse("[{\"id\":41,\"nombre\":\"Investigador\"},{\"id\":3,\"nombre\":\"Administrador\"}]")! : new JsonArray());
        public Task<JsonNode> Crear(string correo, string hash, int[] roles)
        {
            if (User is not null) throw new Npgsql.PostgresException("duplicate", "ERROR", "ERROR", "23505");
            CreatedRoles = roles;
            User = new(71, correo, hash, true, ["Investigador"]);
            return Task.FromResult<JsonNode>(new JsonObject { ["id"] = 71 });
        }
        public Task RegistrarSesion(string jti, long id, DateTimeOffset expira) { sessions.Add(jti); return Task.CompletedTask; }
        public Task<Credencial?> Sesion(string jti, long id) => Task.FromResult(sessions.Contains(jti) && User?.Id == id ? User : null);
        public Task Revocar(string jti, long id) { sessions.Remove(jti); return Task.CompletedTask; }
        public Task<JsonNode> Listar() => throw new NotImplementedException();
        public Task<JsonNode> Editar(long id, string correo, int[] roles) => throw new NotImplementedException();
        public Task<bool> Inactivar(long id) => throw new NotImplementedException();
        public Task Bootstrap(string correo, string hash) => throw new NotImplementedException();
        public Task Restablecer(long id, string hash) => throw new NotImplementedException();
    }
    [Fact]
    public async Task RegistrationHashesPasswordNormalizesEmailAndRestrictsPrivileges()
    {
        var store = new RegistroStore();
        using var factory = TestHostFactory.Create(s => s.AddSingleton<ICuentaRepository>(store), authenticated:false);
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/registro", new { correo="Person@Invalid.test", password=Password });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.Equal("person@invalid.test", store.User!.Correo);
        Assert.StartsWith("$2a$12$", store.User.PasswordHash);
        Assert.True(new PasswordService().Verify(Password, store.User.PasswordHash));
        Assert.Equal([41], store.CreatedRoles);
        Assert.Equal("Investigador", body["usuario"]!["roles"]![0]!.GetValue<string>());
        Assert.DoesNotContain(store.User.PasswordHash, await response.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new("Bearer", body["token"]!.GetValue<string>());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/auth/usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/universidad", new {})).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/logout", new {})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/login", new { correo=store.User.Correo, password=Password })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { correo=store.User.Correo, password="incorrect" })).StatusCode);
    }
    [Theory]
    [InlineData("roles", "[3]")]
    [InlineData("activo", "true")]
    [InlineData("id", "1")]
    public async Task PublicRegistrationRejectsPrivilegeFields(string field, string value)
    {
        var store = new RegistroStore();
        using var factory = TestHostFactory.Create(s => s.AddSingleton<ICuentaRepository>(store), authenticated:false);
        using var client = factory.CreateClient();
        var payload = new JsonObject { ["correo"]="person@invalid.test", ["password"]=Password, [field]=JsonNode.Parse(value) };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/registro", payload)).StatusCode);
        Assert.Null(store.User);
    }
    [Theory]
    [InlineData("not-an-email", "long-enough-password")]
    [InlineData("person@invalid.test", "short")]
    [InlineData("person@invalid.test", "ñññññññññññññññññññññññññññññññññññññ")]
    public async Task InvalidInputDoesNotCreateAccount(string correo, string password)
    {
        var store = new RegistroStore();
        using var factory = TestHostFactory.Create(s => s.AddSingleton<ICuentaRepository>(store), authenticated:false);
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/registro", new { correo, password })).StatusCode);
        Assert.Null(store.User);
    }
    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task DuplicateCannotReplacePasswordOrReactivateAccount(bool active)
    {
        var store = new RegistroStore { User=new(71, "person@invalid.test", new PasswordService().Hash(Password), active, ["Administrador"]) };
        var before = store.User;
        using var factory = TestHostFactory.Create(s => s.AddSingleton<ICuentaRepository>(store), authenticated:false);
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/registro", new { correo="person@invalid.test", password="Another-test-password" })).StatusCode);
        Assert.Same(before, store.User);
        Assert.Null(store.CreatedRoles);
    }
    [Fact]
    public async Task MissingRoleFailsClosed()
    {
        var store = new RegistroStore { RoleAvailable=false };
        using var factory = TestHostFactory.Create(s => s.AddSingleton<ICuentaRepository>(store), authenticated:false);
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/registro", new { correo="person@invalid.test", password=Password })).StatusCode);
        Assert.Null(store.User);
    }
    [Fact]
    public async Task RegistrationAndLoginShareOriginBudget()
    {
        using var factory = TestHostFactory.Create(authenticated:false);
        using var client = factory.CreateClient();
        for (var i=0; i<10; i++)
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(i%2==0 ? "/api/auth/registro" : "/api/auth/login", new {})).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/auth/registro", new {})).StatusCode);
    }

    [RequiresDatabaseFact]
    public async Task RealPostgresPersistsRegistrationAndRejectsDuplicateWithoutReplacingHash()
    {
        using var factory = TestHostFactory.Create(authenticated:false);
        using var client = factory.CreateClient();
        var correo = "signup-" + Guid.NewGuid().ToString("N") + "@invalid.test";
        var response = await client.PostAsJsonAsync("/api/auth/registro", new { correo, password=Password });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICuentaRepository>();
        var stored = (await repository.Buscar(correo))!;
        Assert.True(new PasswordService().Verify(Password, stored.PasswordHash));
        Assert.StartsWith("$2a$12$", stored.PasswordHash);
        Assert.Equal(["Investigador"], stored.Roles);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/registro", new { correo, password="Another-isolated-password" })).StatusCode);
        Assert.Equal(stored.PasswordHash, (await repository.Buscar(correo))!.PasswordHash);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/login", new { correo, password=Password })).StatusCode);
        await repository.Inactivar(stored.Id);
    }
}
