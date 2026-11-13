using System.Collections.Generic;
using System.Threading.Tasks;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

public interface IReporteRepository
{
    Task<IEnumerable<Consulta1GrupoProductividadDto>> GetConsulta1GruposUniversidadLiderLineasAsync();
    Task<IEnumerable<Consulta2SemilleroTutorDto>> GetConsulta2SemillerosGruposTutoresAsync();
    Task<IEnumerable<Consulta3ImpactoOdsDto>> GetConsulta3ImpactoOdsGruposUniversidadAsync();
    Task<IEnumerable<Consulta4CoberturaOcdeDto>> GetConsulta4CoberturaOcdeGruposLiderAsync();
    Task<IEnumerable<Consulta5TransferenciaAplicacionDto>> GetConsulta5TransferenciaAplicacionSemillerosAsync();
    Task<IEnumerable<Consulta6TrayectoriaDocenteDto>> GetConsulta6TrayectoriaDocentesGruposAsync();
    Task<IEnumerable<Consulta7MatrizSemilleroDto>> GetConsulta7MatrizSemillerosLineasOcdeOdsAsync();
    Task<IEnumerable<Consulta8CapacidadUniversidadDto>> GetConsulta8CapacidadCiudadUniversidadAsync();
    Task<IEnumerable<Consulta9MapaIntegralLineaDto>> GetConsulta9MapaIntegralLineasAsync();
    Task<IEnumerable<Consulta10AuditoriaRbacDto>> GetConsulta10AuditoriaSeguridadRbacDocentesAsync();
}
