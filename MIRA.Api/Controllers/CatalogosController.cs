using MIRA.Api.Peticiones;
using MIRA.Api.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MIRA.Api.Controllers;

[ApiController]
[Route("api/catalogos")]
[Authorize]
public sealed class CatalogosController(ICatalogoServicio catalogoServicio) : ControllerBase
{
    [HttpGet("{tipo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar(string tipo, CancellationToken cancellationToken)
    {
        var items = await catalogoServicio.ListarAsync(tipo, cancellationToken);
        return Ok(items);
    }

    [HttpPost("{tipo}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Crear(string tipo, [FromBody] CatalogoItemRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var item = await catalogoServicio.CrearAsync(tipo, request.Nombre, cancellationToken);
        return Created($"/api/catalogos/{tipo}/{item.Id}", item);
    }
}
