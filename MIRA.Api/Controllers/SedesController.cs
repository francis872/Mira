using MIRA.Api.Peticiones;
using MIRA.Api.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MIRA.Api.Controllers;

[ApiController]
[Route("api/sedes")]
[Authorize]
public sealed class SedesController(ISedeServicio sedeServicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var sedes = await sedeServicio.ListarAsync(cancellationToken);
        return Ok(sedes);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Crear([FromBody] CrearSedeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var sede = await sedeServicio.CrearAsync(request.Nombre, request.Ciudad, cancellationToken);
        return Created($"/api/sedes/{sede.Id}", sede);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarSedeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var sede = await sedeServicio.ActualizarAsync(id, request.Nombre, request.Ciudad, request.Activa, cancellationToken);
        return Ok(sede);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        await sedeServicio.DesactivarAsync(id, cancellationToken);
        return NoContent();
    }
}
