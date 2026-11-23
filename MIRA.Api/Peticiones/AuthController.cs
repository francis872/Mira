using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MIRA.Api.Modelos;
using MIRA.Api.Servicios.Seguridad;

namespace MIRA.Api.Peticiones;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService service) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("login")]
    [ProducesResponseType<SesionRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await service.Login(request);
        return result is null ? Unauthorized(new { mensaje="Correo o contraseña incorrectos, o cuenta no disponible." }) : Ok(result);
    }
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { id=long.Parse(User.FindFirstValue("sub")!), correo=User.FindFirstValue("email"), roles=User.FindAll("role").Select(c => c.Value) });
    [HttpPost("logout")]
    public async Task<IActionResult> Logout() { await service.Revocar(User); return Ok(new { mensaje="Sesión cerrada." }); }
    [HttpGet("usuarios"), Authorize(Roles="Administrador")]
    public async Task<IActionResult> Usuarios() => Ok(await service.Listar());
    [HttpGet("roles"), Authorize(Roles="Administrador")]
    public async Task<IActionResult> Roles() => Ok(await service.Roles());
    [HttpPost("usuarios"), Authorize(Roles="Administrador")]
    public async Task<IActionResult> Crear(CrearCuentaRequest request)
    {
        var user = await service.Crear(request);
        return StatusCode(201, user);
    }
    [HttpPut("usuarios/{id:long:min(1)}"), Authorize(Roles="Administrador")]
    public async Task<IActionResult> Editar(long id, EditarCuentaRequest request) => Ok(await service.Editar(id, request));
    [HttpDelete("usuarios/{id:long:min(1)}"), Authorize(Roles="Administrador")]
    public async Task<IActionResult> Inactivar(long id) => await service.Inactivar(id) ? Ok(new { mensaje="Cuenta inactivada." }) : NotFound(new { mensaje="Cuenta no encontrada o inactiva." });
    [HttpPut("usuarios/{id:long:min(1)}/password"), Authorize(Roles="Administrador")]
    public async Task<IActionResult> Restablecer(long id, PasswordRequest request)
    {
        await service.Restablecer(id, request);
        return Ok(new { mensaje="Contraseña restablecida. Las sesiones previas fueron revocadas." });
    }
}
