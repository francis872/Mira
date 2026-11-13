using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIRA.Api.Servicios;

namespace MIRA.Api.Controllers;

/// <summary>
/// Controlador REST para el Dashboard Ejecutivo y las 10 consultas multitabla de la Versión 4 (v0.4).
/// </summary>
[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _service;

    public ReportesController(IReporteService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna las 10 consultas multitabla consolidadas para alimentar el Dashboard con gráficos Chart.js.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _service.ObtenerDashboardCompletoAsync();
        return Ok(dashboard);
    }

    /// <summary>
    /// Ejecuta individualmente cualquiera de las 10 consultas analíticas multitabla (4+ tablas por consulta).
    /// </summary>
    [HttpGet("consulta/{numero:int}")]
    public async Task<IActionResult> GetConsulta(int numero)
    {
        try
        {
            var datos = await _service.EjecutarConsultaMultitablaAsync(numero);
            return Ok(new
            {
                consultaId = numero,
                exito = true,
                datos
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
