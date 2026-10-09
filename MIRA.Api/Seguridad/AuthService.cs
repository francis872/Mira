using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace MIRA.Api.Seguridad;

public sealed class AuthService(IAuthRepository repository, IConfiguration configuration) : IAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await repository.BuscarPorCorreoAsync(request.Correo);
        if (user is null || !user.Activo || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var roles = await repository.RolesUsuarioAsync(user.Id);
        var signingKey = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret no está configurado.");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(ParametrosSeguridad.MinutosDeSesion);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Correo)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Nombre)));

        var token = new JwtSecurityToken(
            ParametrosSeguridad.Emisor,
            ParametrosSeguridad.Audiencia,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256));

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiresAt,
            roles.Select(role => role.Nombre).ToArray());
    }

    public Task<IReadOnlyList<RolItem>> RolesActivosAsync() => repository.RolesActivosAsync();

    public async Task<IReadOnlyList<UsuarioAdminResponse>> ListarUsuariosAsync()
    {
        var json = await repository.ListarAsync();
        return JsonSerializer.Deserialize<List<UsuarioAdminResponse>>(json, JsonOptions) ?? [];
    }

    public async Task<UsuarioAdminResponse?> ConsultarUsuarioAsync(long id)
    {
        if (id <= 0)
        {
            throw new AuthServiceException(StatusCodes.Status400BadRequest, "El identificador debe ser positivo.");
        }

        var json = await repository.ConsultarAsync(id);
        return json is null ? null : JsonSerializer.Deserialize<UsuarioAdminResponse>(json, JsonOptions);
    }

    public async Task<UsuarioAdminResponse> CrearUsuarioAsync(CrearUsuarioRequest request)
    {
        if (!CorreoValido(request.Correo) || !RolesValidos(request.Roles) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
        {
            throw new AuthServiceException(StatusCodes.Status400BadRequest, "Correo, roles o contraseña inválidos (mínimo 12 caracteres).");
        }

        try
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password, ParametrosSeguridad.CostoBcrypt);
            var json = await repository.CrearAsync(request.Correo.Trim(), hash, request.Roles.Distinct().ToArray());
            return JsonSerializer.Deserialize<UsuarioAdminResponse>(json, JsonOptions)
                ?? throw new InvalidOperationException("La rutina de creación devolvió una respuesta vacía.");
        }
        catch (PostgresException exception)
        {
            throw ConvertirError(exception);
        }
    }

    public async Task<UsuarioAdminResponse> ActualizarUsuarioAsync(long id, ActualizarUsuarioRequest request)
    {
        if (id <= 0 || !CorreoValido(request.Correo) || !RolesValidos(request.Roles))
        {
            throw new AuthServiceException(StatusCodes.Status400BadRequest, "Identificador, correo o roles inválidos.");
        }

        try
        {
            var json = await repository.ActualizarAsync(id, request.Correo.Trim(), request.Roles.Distinct().ToArray())
                ?? throw new AuthServiceException(StatusCodes.Status404NotFound, "Usuario no encontrado.");
            return JsonSerializer.Deserialize<UsuarioAdminResponse>(json, JsonOptions)
                ?? throw new InvalidOperationException("La rutina de actualización devolvió una respuesta vacía.");
        }
        catch (PostgresException exception)
        {
            throw ConvertirError(exception);
        }
    }

    public Task<bool> InactivarUsuarioAsync(long id)
    {
        if (id <= 0)
        {
            throw new AuthServiceException(StatusCodes.Status400BadRequest, "El identificador debe ser positivo.");
        }

        return repository.InactivarAsync(id);
    }

    private static bool CorreoValido(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            return false;
        }

        try
        {
            return new MailAddress(correo.Trim()).Address.Equals(correo.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool RolesValidos(int[]? roles) => roles is { Length: > 0 } && roles.All(role => role > 0);

    private static AuthServiceException ConvertirError(PostgresException exception) => exception.SqlState switch
    {
        "23505" => new AuthServiceException(StatusCodes.Status409Conflict, "El correo ya está registrado."),
        "P0002" => new AuthServiceException(StatusCodes.Status404NotFound, "Usuario no encontrado o inactivo."),
        "23503" or "22023" => new AuthServiceException(StatusCodes.Status400BadRequest, "Uno o más roles no existen o la solicitud es inválida."),
        _ => new AuthServiceException(StatusCodes.Status500InternalServerError, "No fue posible completar la operación.")
    };
}