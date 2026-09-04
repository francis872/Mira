using MIRA.Api.Peticiones;
using MIRA.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

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
}
