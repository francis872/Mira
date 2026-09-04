using MIRA.Api.Peticiones;
using MIRA.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MIRA.Api.Controllers;

[ApiController]
[Route("api/publico")]
public sealed class PublicoController(
    ISedeServicio sedeServicio,
    ICatalogoServicio catalogoServicio) : ControllerBase
{
    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Resumen()
    {
        return Ok(new
        {
            proyecto = "MIRA",
            version = "v0.1",
            entorno = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Desconocido",
            horaServidorUtc = DateTime.UtcNow,
            modulos = new[]
            {
                "Estado API",
                "Catalogos",
                "Sedes",
                "Autenticacion"
            }
        });
    }

    [HttpGet("sedes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarSedes(CancellationToken cancellationToken)
    {
        var sedes = await sedeServicio.ListarAsync(cancellationToken);
        return Ok(sedes);
    }

    [HttpPost("sedes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearSede([FromBody] CrearSedeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var sede = await sedeServicio.CrearAsync(request.Nombre, request.Ciudad, cancellationToken);
        return Created($"/api/publico/sedes/{sede.Id}", sede);
    }

    [HttpGet("catalogos/{tipo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarCatalogo(string tipo, CancellationToken cancellationToken)
    {
        var items = await catalogoServicio.ListarAsync(tipo, cancellationToken);
        return Ok(items);
    }

    [HttpPost("catalogos/{tipo}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearCatalogo(string tipo, [FromBody] CatalogoItemRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var item = await catalogoServicio.CrearAsync(tipo, request.Nombre, cancellationToken);
        return Created($"/api/publico/catalogos/{tipo}/{item.Id}", item);
    }
}
