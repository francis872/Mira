using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using MIRA.Api.Servicios;

namespace MIRA.Api.Peticiones;

[ApiController]
[Route("api/investigacion")]
public sealed class InvestigacionController(InvestigacionService service) : ControllerBase
{
    private IActionResult Resultado(JsonNode? dato) => dato is null
        ? NotFound(new { mensaje="Registro no encontrado o inactivo." }) : Ok(dato);

    [HttpGet("referencias/usuarios")]
    public async Task<IActionResult> Usuarios() => Ok(await service.Usuarios());
    [HttpGet("grupos/ficha/{id:int:min(1)}")]
    public async Task<IActionResult> Ficha(int id) => Resultado(await service.Ficha(id));
    [HttpPost("grupos/ficha")]
    public async Task<IActionResult> CrearFicha([FromBody] JsonNode datos)
    {
        var creado = await service.GuardarFicha(null,datos);
        return CreatedAtAction(nameof(Ficha), new { id=creado!["grupo"]!["id"]!.GetValue<int>() }, creado);
    }
    [HttpPut("grupos/ficha/{id:int:min(1)}")]
    public async Task<IActionResult> ActualizarFicha(int id, [FromBody] JsonNode datos) => Resultado(await service.GuardarFicha(id,datos));

    [HttpGet("{recurso}")]
    public async Task<IActionResult> Listar(string recurso) => Ok(await service.Listar(recurso));
    [HttpGet("{recurso}/{id:int:min(1)}")]
    public async Task<IActionResult> Obtener(string recurso, int id) => Resultado(await service.Obtener(recurso,id));
    [HttpGet("{recurso}/{izquierda:int:min(1)}/{derecha:int:min(1)}")]
    public async Task<IActionResult> ObtenerPuente(string recurso, int izquierda, int derecha) => Resultado(await service.ObtenerPuente(recurso,izquierda,derecha));
    [HttpPost("{recurso}")]
    public async Task<IActionResult> Crear(string recurso, [FromBody] JsonNode datos)
    {
        var definicion = service.Recurso(recurso);
        var creado = await service.Crear(recurso,datos);
        return definicion.EsPuente
            ? CreatedAtAction(nameof(ObtenerPuente),new { recurso, izquierda=creado![definicion.Claves![0]]!.GetValue<int>(), derecha=creado[definicion.Claves[1]]!.GetValue<int>() },creado)
            : CreatedAtAction(nameof(Obtener),new { recurso, id=creado!["id"]!.GetValue<int>() },creado);
    }
    [HttpPut("{recurso}/{id:int:min(1)}")]
    public async Task<IActionResult> Actualizar(string recurso, int id, [FromBody] JsonNode datos) => Resultado(await service.Actualizar(recurso,id,datos));
    [HttpDelete("{recurso}/{id:int:min(1)}")]
    public async Task<IActionResult> Eliminar(string recurso, int id) => await service.Eliminar(recurso,id)
        ? Ok(new { mensaje="Registro retirado correctamente." }) : NotFound(new { mensaje="Registro no encontrado o inactivo." });
    [HttpDelete("{recurso}/{izquierda:int:min(1)}/{derecha:int:min(1)}")]
    public async Task<IActionResult> RetirarPuente(string recurso, int izquierda, int derecha) => await service.Eliminar(recurso,izquierda,derecha)
        ? Ok(new { mensaje="Asociación retirada correctamente." }) : NotFound(new { mensaje="Asociación no encontrada." });
}
