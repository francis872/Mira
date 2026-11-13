using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios;

namespace MIRA.Api.Servicios;

/// <summary>
/// Servicio de lógica de negocio para la ejecución y validación de las 10 consultas multitabla (v0.4).
/// </summary>
public class ReporteService : IReporteService
{
    private readonly IReporteRepository _repository;

    public ReporteService(IReporteRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardEjecutivoDto> ObtenerDashboardCompletoAsync()
    {
        return new DashboardEjecutivoDto
        {
            Consulta1GruposProductividad = await _repository.GetConsulta1GruposUniversidadLiderLineasAsync(),
            Consulta2SemillerosTutores = await _repository.GetConsulta2SemillerosGruposTutoresAsync(),
            Consulta3ImpactoOds = await _repository.GetConsulta3ImpactoOdsGruposUniversidadAsync(),
            Consulta4CoberturaOcde = await _repository.GetConsulta4CoberturaOcdeGruposLiderAsync(),
            Consulta5TransferenciaAplicacion = await _repository.GetConsulta5TransferenciaAplicacionSemillerosAsync(),
            Consulta6TrayectoriaDocentes = await _repository.GetConsulta6TrayectoriaDocentesGruposAsync(),
            Consulta7MatrizSemilleros = await _repository.GetConsulta7MatrizSemillerosLineasOcdeOdsAsync(),
            Consulta8CapacidadUniversidades = await _repository.GetConsulta8CapacidadCiudadUniversidadAsync(),
            Consulta9MapaIntegralLineas = await _repository.GetConsulta9MapaIntegralLineasAsync(),
            Consulta10AuditoriaRbac = await _repository.GetConsulta10AuditoriaSeguridadRbacDocentesAsync()
        };
    }

    public async Task<object> EjecutarConsultaMultitablaAsync(int numero)
    {
        if (numero <= 0)
            throw new ArgumentException("El número de consulta debe ser un entero positivo entre 1 y 10.");

        return numero switch
        {
            1 => await _repository.GetConsulta1GruposUniversidadLiderLineasAsync(),
            2 => await _repository.GetConsulta2SemillerosGruposTutoresAsync(),
            3 => await _repository.GetConsulta3ImpactoOdsGruposUniversidadAsync(),
            4 => await _repository.GetConsulta4CoberturaOcdeGruposLiderAsync(),
            5 => await _repository.GetConsulta5TransferenciaAplicacionSemillerosAsync(),
            6 => await _repository.GetConsulta6TrayectoriaDocentesGruposAsync(),
            7 => await _repository.GetConsulta7MatrizSemillerosLineasOcdeOdsAsync(),
            8 => await _repository.GetConsulta8CapacidadCiudadUniversidadAsync(),
            9 => await _repository.GetConsulta9MapaIntegralLineasAsync(),
            10 => await _repository.GetConsulta10AuditoriaSeguridadRbacDocentesAsync(),
            _ => throw new KeyNotFoundException($"La consulta multitabla #{numero} no existe. El rango válido es de 1 a 10.")
        };
    }
}
