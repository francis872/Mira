using BCrypt.Net;
using MIRA.Api.Configuracion;
using MIRA.Api.Excepciones;
using MIRA.Api.Repositorios;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MIRA.Api.Servicios;

public sealed class AuthServicio(
    IUsuarioRepositorio usuarioRepositorio,
    IOptions<JwtOptions> jwtOptions) : IAuthServicio
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResultado> LoginAsync(string correo, string password, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.ObtenerPorCorreoAsync(correo, cancellationToken);
        if (usuario is null)
        {
            throw new ApiException("Credenciales inválidas.", StatusCodes.Status401Unauthorized);
        }

        var passwordValido = EsPasswordValido(password, usuario.PasswordHash);
        if (!passwordValido)
        {
            throw new ApiException("Credenciales inválidas.", StatusCodes.Status401Unauthorized);
        }

        var expiration = DateTime.UtcNow.AddHours(_jwtOptions.ExpirationHours);
        var token = CrearToken(usuario, expiration);

        return new AuthResultado
        {
            Token = token,
            ExpiraEnUtc = expiration,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Roles = usuario.Roles
        };
    }

    public Task<IReadOnlyList<string>> ObtenerRolesAsync(int usuarioId, CancellationToken cancellationToken)
        => usuarioRepositorio.ObtenerRolesAsync(usuarioId, cancellationToken);

    public async Task AsignarRolAsync(int usuarioId, string rol, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rol))
        {
            throw new ApiException("El rol es requerido.");
        }

        var created = await usuarioRepositorio.AsignarRolAsync(usuarioId, rol.Trim(), cancellationToken);
        if (!created)
        {
            throw new ApiException("No fue posible asignar el rol. Verifica usuario y rol.", StatusCodes.Status404NotFound);
        }
    }

    public async Task RemoverRolAsync(int usuarioId, string rol, CancellationToken cancellationToken)
    {
        var roles = await usuarioRepositorio.ObtenerRolesAsync(usuarioId, cancellationToken);
        if (roles.Count <= 1 && roles.Contains(rol, StringComparer.OrdinalIgnoreCase))
        {
            throw new ApiException("El usuario debe conservar al menos un rol.");
        }

        var removed = await usuarioRepositorio.RemoverRolAsync(usuarioId, rol, cancellationToken);
        if (!removed)
        {
            throw new ApiException("No fue posible remover el rol.", StatusCodes.Status404NotFound);
        }
    }

    private bool EsPasswordValido(string passwordPlano, string passwordHash)
    {
        if (passwordHash.StartsWith("$2", StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(passwordPlano, passwordHash);
        }

        // Bootstrap fallback: si la semilla tiene password plano, se permite solo en desarrollo local.
        return passwordPlano == passwordHash;
    }

    private string CrearToken(Modelos.Usuario usuario, DateTime expiration)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Correo)
        };
        claims.AddRange(usuario.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
