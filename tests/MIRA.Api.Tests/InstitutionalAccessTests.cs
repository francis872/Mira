using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios.Seguridad;
using MIRA.Api.Servicios.Seguridad;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class InstitutionalAccessTests
{
    private const string Password = "Isolated-institutional-test-only!";
    private sealed class Store : ICuentaRepository
    {
        public Credencial? User;
        public bool RoleAvailable = true;
        public int Creates, Resets;
        public int[]? AssignedRoles;
        public HashSet<string> Sessions = [];
        public Task<Credencial?> Buscar(string correo) => Task.FromResult(User?.Correo.Trim().ToLowerInvariant() == correo ? User : null);
        public Task<JsonNode> Roles() => Task.FromResult<JsonNode>(RoleAvailable ? JsonNode.Parse("[{\"id\":41,\"nombre\":\"Investigador\"}]")! : new JsonArray());
        public Task<JsonNode> Crear(string correo,string hash,int[] roles)
        {
            if (User is not null) throw new InvalidOperationException("Account already exists");
            Creates++; AssignedRoles=roles; User=new(71,correo,hash,true,["Investigador"]);
            return Task.FromResult<JsonNode>(new JsonObject{["id"]=71});
        }
        public Task Restablecer(long id,string hash) { Resets++;User=User! with {PasswordHash=hash};Sessions.Clear();return Task.CompletedTask; }
        public Task RegistrarSesion(string jti,long id,DateTimeOffset expira) { Sessions.Add(jti);return Task.CompletedTask; }
        public Task<Credencial?> Sesion(string jti,long id) => Task.FromResult(Sessions.Contains(jti)&&User?.Id==id?User:null);
        public Task Revocar(string jti,long id) { Sessions.Remove(jti);return Task.CompletedTask; }
        public Task<JsonNode> Listar() => throw new NotImplementedException();
        public Task<JsonNode> Editar(long id,string correo,int[] roles) => throw new NotImplementedException();
        public Task<bool> Inactivar(long id) => throw new NotImplementedException();
        public Task Bootstrap(string correo,string hash) => throw new NotImplementedException();
    }
    private static AuthService Service(Store store) => new(store,new PasswordService(),new JwtSettings{Key="isolated-institutional-tests-key-never-deploy-32"});

    [Theory]
    [InlineData("registro")] [InlineData("register")] [InlineData("signup")]
    [InlineData("provision-institutional")] [InlineData("reset-institutional-password")]
    public async Task PublicAndLegacyRegistrationEndpointsDoNotExist(string path)
    {
        var store=new Store();using var factory=TestHostFactory.Create(s=>s.AddSingleton<ICuentaRepository>(store),authenticated:false);
        using var client=factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync("/api/auth/"+path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await client.PostAsJsonAsync("/api/auth/"+path,new {correo="test@invalid.test",password=Password})).StatusCode);
        Assert.Equal(0,store.Creates);
    }
    [Fact]
    public async Task AdministrativeProvisioningRequiresAuthentication()
    {
        using var factory=TestHostFactory.Create(authenticated:false);using var client=factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/auth/usuarios",new {correo="test@invalid.test",password=Password,roles=new[]{1}})).StatusCode);
    }
    [Fact]
    public async Task ExplicitProvisioningHashesPasswordAndResolvesExistingRole()
    {
        var store=new Store();var account=await Service(store).ProvisionarInstitucional(" Person@Invalid.test ",Password,"Investigador");
        Assert.Equal("person@invalid.test",account.Correo);Assert.True(account.Activo);
        Assert.Equal([41],store.AssignedRoles);Assert.Equal(["Investigador"],account.Roles);
        Assert.StartsWith("$2a$12$",store.User!.PasswordHash);
        Assert.True(new PasswordService().Verify(Password,store.User.PasswordHash));
        Assert.DoesNotContain(store.User.PasswordHash,System.Text.Json.JsonSerializer.Serialize(account));
    }
    [Fact]
    public async Task RepeatProvisioningPreservesIdentityHashRolesAndSessions()
    {
        var store=new Store();var service=Service(store);await service.ProvisionarInstitucional("person@invalid.test",Password,"Investigador");
        store.Sessions.Add("existing-session");var original=store.User;
        await Service(store).ProvisionarInstitucional("PERSON@invalid.test",Password,"Investigador");
        Assert.Same(original,store.User);Assert.Equal(1,store.Creates);Assert.Equal(0,store.Resets);Assert.Contains("existing-session",store.Sessions);
    }
    [Fact]
    public async Task MismatchedPasswordNeverResetsImplicitly()
    {
        var store=new Store();var service=Service(store);await service.ProvisionarInstitucional("person@invalid.test",Password,"Investigador");var original=store.User;
        await Assert.ThrowsAsync<SeguridadValidationException>(()=>service.ProvisionarInstitucional("person@invalid.test","Different-isolated-password!","Investigador"));
        Assert.Same(original,store.User);Assert.Equal(0,store.Resets);
    }
    [Fact]
    public async Task ExplicitResetPreservesIdentityAndRevokesSessions()
    {
        var store=new Store();var service=Service(store);await service.ProvisionarInstitucional("person@invalid.test",Password,"Investigador");
        var before=store.User!;store.Sessions.Add("existing-session");
        await service.ProvisionarInstitucional("person@invalid.test","Replacement-isolated-password!","Investigador",resetPassword:true);
        Assert.Equal(before.Id,store.User!.Id);Assert.Equal(before.Roles,store.User.Roles);Assert.Equal(1,store.Resets);Assert.Empty(store.Sessions);
        Assert.False(new PasswordService().Verify(Password,store.User.PasswordHash));
        Assert.True(new PasswordService().Verify("Replacement-isolated-password!",store.User.PasswordHash));
    }
    [Theory]
    [InlineData("Administrador")] [InlineData("MAESTRO")] [InlineData("unknown")]
    public async Task UnsupportedOrGlobalAdministratorRoleCannotBeProvisioned(string role)
    {
        var store=new Store();await Assert.ThrowsAsync<SeguridadValidationException>(()=>Service(store).ProvisionarInstitucional("person@invalid.test",Password,role));Assert.Null(store.User);
    }
    [Theory]
    [InlineData(false,"Investigador")] [InlineData(true,"Administrador")] [InlineData(true,"Coordinador")]
    public async Task ExistingIncompatibleAccountRemainsUnchanged(bool active,string role)
    {
        var store=new Store{User=new(71,"person@invalid.test",new PasswordService().Hash(Password),active,[role])};var original=store.User;
        await Assert.ThrowsAsync<SeguridadValidationException>(()=>Service(store).ProvisionarInstitucional("person@invalid.test",Password,"Investigador",resetPassword:true));
        Assert.Same(original,store.User);Assert.Equal(0,store.Creates);Assert.Equal(0,store.Resets);
    }
    [Fact]
    public async Task MissingSecretAndMissingRoleFailWithoutCreatingAccounts()
    {
        var store=new Store();await Assert.ThrowsAsync<SeguridadValidationException>(()=>Service(store).ProvisionarInstitucional("person@invalid.test","","Investigador"));
        store.RoleAvailable=false;await Assert.ThrowsAsync<SeguridadValidationException>(()=>Service(store).ProvisionarInstitucional("person@invalid.test",Password,"Investigador"));
        Assert.Null(store.User);
    }
    [RequiresDatabaseFact]
    public async Task RealPostgresProvisioningIsIdempotentAndExplicitResetRevokesLogin()
    {
        using var factory=TestHostFactory.Create(authenticated:false);using var scope=factory.Services.CreateScope();
        var service=scope.ServiceProvider.GetRequiredService<AuthService>();var repository=scope.ServiceProvider.GetRequiredService<ICuentaRepository>();
        var correo="institutional-"+Guid.NewGuid().ToString("N")+"@invalid.test";
        var user=await service.ProvisionarInstitucional(correo,Password,"Investigador");var before=(await repository.Buscar(correo))!;
        await service.ProvisionarInstitucional(correo.ToUpperInvariant(),Password,"Investigador");
        var repeat=(await repository.Buscar(correo))!;Assert.Equal(before.Id,repeat.Id);Assert.Equal(before.PasswordHash,repeat.PasswordHash);
        using var client=factory.CreateClient();var response=await client.PostAsJsonAsync("/api/auth/login",new {correo,password=Password});Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var body=(await response.Content.ReadFromJsonAsync<JsonObject>())!;client.DefaultRequestHeaders.Authorization=new("Bearer",body["token"]!.GetValue<string>());
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.GetAsync("/api/auth/usuarios")).StatusCode);
        await service.ProvisionarInstitucional(correo,"Explicit-reset-test-password!","Investigador",resetPassword:true);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(user.Id,(await repository.Buscar(correo))!.Id);await repository.Inactivar(user.Id);
    }
}
