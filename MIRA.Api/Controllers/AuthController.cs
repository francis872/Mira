using MIRA.Api.Peticiones;
using MIRA.Api.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MIRA.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthServicio authServicio) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var resultado = await authServicio.LoginAsync(request.Correo, request.Password, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Token inválido." });
        }

        var roles = await authServicio.ObtenerRolesAsync(userId, cancellationToken);
        return Ok(new
        {
            Id = userId,
            Nombre = User.FindFirstValue(ClaimTypes.Name),
            Correo = User.FindFirstValue(ClaimTypes.Email),
            Roles = roles
        });
    }

    [HttpPost("usuarios/{usuarioId:int}/roles")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AsignarRol(int usuarioId, [FromBody] AsignarRolRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await authServicio.AsignarRolAsync(usuarioId, request.Rol, cancellationToken);
        return NoContent();
    }

    [HttpDelete("usuarios/{usuarioId:int}/roles/{rol}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoverRol(int usuarioId, string rol, CancellationToken cancellationToken)
    {
        await authServicio.RemoverRolAsync(usuarioId, rol, cancellationToken);
        return NoContent();
    }
}
