using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using MIRA.Api.Seguridad;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class AuthTests
{
    private static string CrearToken(string role) => TestHostFactory.Token(role);

    [Fact]
    public async Task AdminRoutes_RequireAuthentication()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUserWithoutAdminRole_ReceivesForbidden()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CrearToken("Investigador"));

        var response = await client.GetAsync("/api/auth/roles");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanReadActiveRoles()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CrearToken("Administrador"));

        var response = await client.GetAsync("/api/auth/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Administrador", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task InvalidCredentials_ReturnUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody@example.invalid", "InvalidPassword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginVerifiesBcryptAndReturnsRoleClaims()
    {
        var repository = new FakeAuthRepository
        {
            User = new UsuarioAuth(8, "researcher@example.invalid", BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!", 4), true),
            UserRoles = [new RolItem(2, "Investigador")]
        };
        var service = new AuthService(repository, CreateConfiguration());

        var response = await service.LoginAsync(new LoginRequest("researcher@example.invalid", "CorrectPassword123!"));
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response!.AccessToken);

        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Investigador");
        Assert.Equal("Bearer", response.TokenType);
    }

    [Fact]
    public async Task UserCreationStoresBcryptHashInsteadOfPassword()
    {
        var repository = new FakeAuthRepository();
        var service = new AuthService(repository, CreateConfiguration());

        await service.CrearUsuarioAsync(new CrearUsuarioRequest("new@example.invalid", "CorrectPassword123!", [1]));

        Assert.NotNull(repository.CreatedHash);
        Assert.NotEqual("CorrectPassword123!", repository.CreatedHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("CorrectPassword123!", repository.CreatedHash));
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return TestHostFactory.Create(services =>
        {
            services.RemoveAll<IAuthService>();
            services.AddSingleton<IAuthService>(new FakeAuthService());
        });
    }

    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Secret"] = TestHostFactory.JwtSecret })
        .Build();

    private sealed class FakeAuthService : IAuthService
    {
        public Task<LoginResponse?> LoginAsync(LoginRequest request) => Task.FromResult<LoginResponse?>(null);
        public Task<IReadOnlyList<RolItem>> RolesActivosAsync() => Task.FromResult<IReadOnlyList<RolItem>>([new RolItem(1, "Administrador")]);
        public Task<IReadOnlyList<UsuarioAdminResponse>> ListarUsuariosAsync() => Task.FromResult<IReadOnlyList<UsuarioAdminResponse>>([]);
        public Task<UsuarioAdminResponse?> ConsultarUsuarioAsync(long id) => Task.FromResult<UsuarioAdminResponse?>(null);
        public Task<UsuarioAdminResponse> CrearUsuarioAsync(CrearUsuarioRequest request) => throw new NotImplementedException();
        public Task<UsuarioAdminResponse> ActualizarUsuarioAsync(long id, ActualizarUsuarioRequest request) => throw new NotImplementedException();
        public Task<bool> InactivarUsuarioAsync(long id) => Task.FromResult(false);
    }

    private sealed class FakeAuthRepository : IAuthRepository
    {
        public UsuarioAuth? User { get; init; }
        public IReadOnlyList<RolItem> UserRoles { get; init; } = [];
        public string? CreatedHash { get; private set; }

        public Task<UsuarioAuth?> BuscarPorCorreoAsync(string correo) => Task.FromResult(User);
        public Task<IReadOnlyList<RolItem>> RolesUsuarioAsync(long id) => Task.FromResult(UserRoles);
        public Task<IReadOnlyList<RolItem>> RolesActivosAsync() => Task.FromResult<IReadOnlyList<RolItem>>([new RolItem(1, "Administrador")]);
        public Task<string> CrearAsync(string correo, string hash, int[] roles)
        {
            CreatedHash = hash;
            return Task.FromResult("{\"id\":4,\"correo\":\"new@example.invalid\",\"activo\":true,\"roles\":[{\"id\":1,\"nombre\":\"Administrador\"}]}");
        }
        public Task<string?> ActualizarAsync(long id, string correo, int[] roles) => Task.FromResult<string?>(null);
        public Task<string?> ConsultarAsync(long id) => Task.FromResult<string?>(null);
        public Task<string> ListarAsync() => Task.FromResult("[]");
        public Task<bool> InactivarAsync(long id) => Task.FromResult(false);
        public Task<string> BootstrapAdminAsync(string correo, string passwordHash) => Task.FromResult("{}");
    }
}