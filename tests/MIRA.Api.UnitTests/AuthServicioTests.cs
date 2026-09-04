using Microsoft.Extensions.Options;
using MIRA.Api.Configuracion;
using MIRA.Api.Excepciones;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios;
using MIRA.Api.Servicios;

namespace MIRA.Api.UnitTests;

public class AuthServicioTests
{
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var repo = new UsuarioRepositorioFake(new Usuario
        {
            Id = 1,
            Nombre = "Admin",
            Correo = "admin@mira.local",
            PasswordHash = "Admin123!",
            Roles = ["Administrador"]
        });
        var options = Options.Create(new JwtOptions
        {
            Key = "12345678901234567890123456789012",
            Issuer = "tests",
            Audience = "tests",
            ExpirationHours = 1
        });

        var service = new AuthServicio(repo, options);
        var result = await service.LoginAsync("admin@mira.local", "Admin123!", CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Contains("Administrador", result.Roles);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorized()
    {
        var repo = new UsuarioRepositorioFake(new Usuario
        {
            Id = 1,
            Nombre = "Admin",
            Correo = "admin@mira.local",
            PasswordHash = "Admin123!",
            Roles = ["Administrador"]
        });
        var options = Options.Create(new JwtOptions
        {
            Key = "12345678901234567890123456789012",
            Issuer = "tests",
            Audience = "tests"
        });

        var service = new AuthServicio(repo, options);

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            service.LoginAsync("admin@mira.local", "bad", CancellationToken.None));

        Assert.Equal(StatusCodes.Status401Unauthorized, ex.StatusCode);
    }

    private sealed class UsuarioRepositorioFake(Usuario? usuario) : IUsuarioRepositorio
    {
        private readonly List<string> _roles = usuario?.Roles.ToList() ?? [];

        public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken)
            => Task.FromResult(usuario?.Correo == correo ? usuario : null);

        public Task<IReadOnlyList<string>> ObtenerRolesAsync(int usuarioId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<string>>(_roles);

        public Task<bool> AsignarRolAsync(int usuarioId, string rol, CancellationToken cancellationToken)
        {
            if (_roles.Contains(rol, StringComparer.OrdinalIgnoreCase))
            {
                return Task.FromResult(false);
            }

            _roles.Add(rol);
            return Task.FromResult(true);
        }

        public Task<bool> RemoverRolAsync(int usuarioId, string rol, CancellationToken cancellationToken)
        {
            var removed = _roles.RemoveAll(x => string.Equals(x, rol, StringComparison.OrdinalIgnoreCase)) > 0;
            return Task.FromResult(removed);
        }
    }
}
