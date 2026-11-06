using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios.Seguridad;

namespace MIRA.Api.Servicios.Seguridad;

public sealed class SeguridadValidationException(string message) : Exception(message);

public sealed class PasswordService
{
    // Trabajo equivalente para cuentas inexistentes, sin aceptar credenciales heredadas inseguras.
    private static readonly string Dummy = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), 12);
    public string Hash(string password)
    {
        if (password.Length < 12 || Encoding.UTF8.GetByteCount(password) > 72)
            throw new SeguridadValidationException("La contraseña debe tener al menos 12 caracteres y como máximo 72 bytes UTF-8.");
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }
    public bool Verify(string password, string? hash)
    {
        if (Encoding.UTF8.GetByteCount(password) > 72) return false;
        var valid = hash is not null && System.Text.RegularExpressions.Regex.IsMatch(hash, @"^\$2[aby]\$(0[4-9]|1[0-6])\$[./A-Za-z0-9]{53}$");
        try { return BCrypt.Net.BCrypt.Verify(password, valid ? hash! : Dummy) && valid; }
        catch (BCrypt.Net.SaltParseException) { return false; }
    }
}

public sealed class JwtSettings
{
    public required string Key { get; init; }
    public string Issuer { get; init; } = "MIRA";
    public string Audience { get; init; } = "MIRA.Frontend";
    public int Minutes { get; init; } = 20;
    public TokenValidationParameters Validation() => new()
    {
        ValidateIssuerSigningKey=true, IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
        ValidateIssuer=true, ValidIssuer=Issuer, ValidateAudience=true, ValidAudience=Audience,
        ValidateLifetime=true, RequireExpirationTime=true, RequireSignedTokens=true,
        ValidAlgorithms=[SecurityAlgorithms.HmacSha256], ClockSkew=TimeSpan.Zero,
        NameClaimType="email", RoleClaimType="role"
    };
}

public sealed class AuthService(ICuentaRepository repository, PasswordService passwords, JwtSettings settings)
{
    public static readonly string[] RolesPermitidos = ["Administrador", "Coordinador", "Investigador"];
    public async Task<SesionRespuesta?> Login(LoginRequest request)
    {
        var user = await repository.Buscar(request.Correo.Trim().ToLowerInvariant());
        var verified = passwords.Verify(request.Password, user?.PasswordHash);
        if (!verified || user is null || !user.Activo || !user.Roles.Intersect(RolesPermitidos).Any()) return null;
        return await GenerarJwt(user);
    }
    public async Task<SesionRespuesta> GenerarJwt(Credencial user)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(settings.Minutes);
        var jti = Guid.NewGuid().ToString("N");
        List<Claim> claims = [new("sub", user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new("email", user.Correo), new("jti", jti), new("iat", now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)];
        claims.AddRange(user.Roles.Select(role => new Claim("role", role)));
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims, now.UtcDateTime, expires.UtcDateTime,
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256));
        await repository.RegistrarSesion(jti, user.Id, expires);
        return new(new JwtSecurityTokenHandler().WriteToken(token), expires, new(user.Id, user.Correo, user.Activo, user.Roles));
    }
    public async Task<bool> ValidarSesion(ClaimsPrincipal principal)
    {
        if (!long.TryParse(principal.FindFirstValue("sub"), out var id)) return false;
        var jti = principal.FindFirstValue("jti");
        if (string.IsNullOrEmpty(jti)) return false;
        var current = await repository.Sesion(jti, id);
        return current is { Activo: true } && current.Correo == principal.FindFirstValue("email") &&
            current.Roles.ToHashSet().SetEquals(principal.FindAll("role").Select(c => c.Value));
    }
    public Task Revocar(ClaimsPrincipal principal) => repository.Revocar(principal.FindFirstValue("jti")!, long.Parse(principal.FindFirstValue("sub")!));
    public Task<System.Text.Json.Nodes.JsonNode> Crear(CrearCuentaRequest request) => repository.Crear(request.Correo.Trim().ToLowerInvariant(), passwords.Hash(request.Password), request.Roles);
    public Task<System.Text.Json.Nodes.JsonNode> Editar(long id, EditarCuentaRequest request) => repository.Editar(id, request.Correo.Trim().ToLowerInvariant(), request.Roles);
    public Task<System.Text.Json.Nodes.JsonNode> Listar() => repository.Listar();
    public Task<System.Text.Json.Nodes.JsonNode> Roles() => repository.Roles();
    public Task<bool> Inactivar(long id) => repository.Inactivar(id);
    public Task Restablecer(long id, PasswordRequest request) => repository.Restablecer(id, passwords.Hash(request.Password));
    public Task Bootstrap(string correo, string password)
    {
        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(correo) || correo.Length > 254)
            throw new SeguridadValidationException("Correo inválido.");
        return repository.Bootstrap(correo.Trim().ToLowerInvariant(), passwords.Hash(password));
    }
    // Recuperación operativa local: no se expone por HTTP y exige acceso a la BD.
    public async Task RestablecerAdmin(string correo, string password)
    {
        var account = await repository.Buscar(correo.Trim().ToLowerInvariant());
        if (account is not { Activo: true } || !account.Roles.Contains("Administrador"))
            throw new SeguridadValidationException("No existe un administrador activo para ese correo.");
        await repository.Restablecer(account.Id, passwords.Hash(password));
    }
}
