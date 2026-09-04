using MIRA.Api.Peticiones;
using MIRA.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MIRA.Api.Controllers;

[ApiController]
[Route("api/catalogos")]
public sealed class CatalogosController(ICatalogoServicio catalogoServicio) : ControllerBase
{
    [HttpGet("{tipo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(string tipo, CancellationToken cancellationToken)
    {
        var items = await catalogoServicio.ListarAsync(tipo, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{tipo}/{id:int}")]
    public async Task<IActionResult> Obtener(string tipo, int id, CancellationToken cancellationToken)
        => Ok(await catalogoServicio.ObtenerPorIdAsync(tipo, id, cancellationToken));

    [HttpPost("{tipo}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
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

    [HttpPut("{tipo}/{id:int}")]
    public async Task<IActionResult> Actualizar(string tipo, int id, [FromBody] CatalogoItemRequest request, CancellationToken cancellationToken)
        => Ok(await catalogoServicio.ActualizarAsync(tipo, id, request.Nombre, request.Activo, cancellationToken));

    [HttpPatch("{tipo}/{id:int}")]
    public async Task<IActionResult> ActualizarParcial(string tipo, int id, [FromBody] CatalogoItemPatchRequest request, CancellationToken cancellationToken)
    {
        var actual = await catalogoServicio.ObtenerPorIdAsync(tipo, id, cancellationToken);
        var nombre = request.Nombre ?? actual.Nombre;
        var activo = request.Activo ?? actual.Activo;
        return Ok(await catalogoServicio.ActualizarAsync(tipo, id, nombre, activo, cancellationToken));
    }

    [HttpDelete("{tipo}/{id:int}")]
    public async Task<IActionResult> Desactivar(string tipo, int id, CancellationToken cancellationToken)
    {
        await catalogoServicio.DesactivarAsync(tipo, id, cancellationToken);
        return NoContent();
    }
}
