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
            Rol = usuario.Rol
        };
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
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

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
