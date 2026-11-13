using System;
using System.Collections.Generic;

namespace MIRA.Api.Modelos;

// =========================================================================
// Modelos y DTOs de la Versión 4 (v0.4): 10 Consultas Multitabla (4+ tablas)
// =========================================================================

/// <summary>
/// Consulta 1 (5 tablas: grupo_investigacion + universidad + docente + grupo_linea + linea_investigacion).
/// Productividad y articulación temática de grupos por universidad sede y docente líder.
/// </summary>
public class Consulta1GrupoProductividadDto
{
    public int GrupoId { get; set; }
    public string CodigoGrupo { get; set; } = string.Empty;
    public string GrupoNombre { get; set; } = string.Empty;
    public string CategoriaGrupo { get; set; } = string.Empty;
    public string UniversidadNombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string DocenteLider { get; set; } = string.Empty;
    public string CorreoLider { get; set; } = string.Empty;
    public int TotalLineas { get; set; }
    public string LineasArticuladas { get; set; } = string.Empty;
}

/// <summary>
/// Consulta 2 (5 tablas: semillero + grupo_investigacion + universidad + docente_semillero + docente).
/// Ecosistema de semilleros activos con su grupo matriz, universidad sede y docente tutor por periodo.
/// </summary>
public class Consulta2SemilleroTutorDto
{
    public int SemilleroId { get; set; }
    public string SemilleroNombre { get; set; } = string.Empty;
    public string GrupoNombre { get; set; } = string.Empty;
    public string CodigoGrupo { get; set; } = string.Empty;
    public string UniversidadNombre { get; set; } = string.Empty;
    public string DocenteTutor { get; set; } = string.Empty;
    public string EscalafonTutor { get; set; } = string.Empty;
    public string RolTutor { get; set; } = string.Empty;
    public string PeriodoAcademico { get; set; } = string.Empty;
}

/// <summary>
/// Consulta 3 (6 tablas: objetivo_desarrollo_sostenible + linea_ods + linea_investigacion + grupo_linea + grupo_investigacion + universidad).
/// Impacto en Objetivos de Desarrollo Sostenible (ODS) articulado por líneas, grupos y universidades.
/// </summary>
public class Consulta3ImpactoOdsDto
{
    public int OdsId { get; set; }
    public string OdsNombre { get; set; } = string.Empty;
    public string CategoriaOds { get; set; } = string.Empty;
    public string LineaNombre { get; set; } = string.Empty;
    public string GrupoNombre { get; set; } = string.Empty;
    public string UniversidadNombre { get; set; } = string.Empty;
    public int TotalGruposImpactados { get; set; }
}

/// <summary>
/// Consulta 4 (6 tablas: area_conocimiento + linea_area + linea_investigacion + grupo_linea + grupo_investigacion + docente).
/// Cobertura de Áreas OCDE por línea de investigación, grupo y docente líder.
/// </summary>
public class Consulta4CoberturaOcdeDto
{
    public int AreaId { get; set; }
    public string GranArea { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
    public string LineaNombre { get; set; } = string.Empty;
    public string GrupoNombre { get; set; } = string.Empty;
    public string DocenteLider { get; set; } = string.Empty;
}

/// <summary>
/// Consulta 5 (6 tablas: area_aplicacion + linea_area_aplicacion + linea_investigacion + semillero_linea + semillero + grupo_investigacion).
/// Transferencia tecnológica por sector de aplicación, línea, semillero y grupo matriz.
/// </summary>
public class Consulta5TransferenciaAplicacionDto
{
    public int AplicacionId { get; set; }
    public string AreaAplicacion { get; set; } = string.Empty;
    public string LineaNombre { get; set; } = string.Empty;
    public string SemilleroNombre { get; set; } = string.Empty;
    public string GrupoMatriz { get; set; } = string.Empty;
    public string CategoriaGrupo { get; set; } = string.Empty;
}

/// <summary>
/// Consulta 6 (5 tablas: docente + linea_investigacion + docente_grupo + grupo_investigacion + universidad).
/// Trayectoria histórica de docentes investigadores en grupos, universidades y su línea matriz.
/// </summary>
public class Consulta6TrayectoriaDocenteDto
{
    public int Cedula { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
    public string Escalafon { get; set; } = string.Empty;
    public string CategoriaMinciencias { get; set; } = string.Empty;
    public string LineaMatriz { get; set; } = string.Empty;
    public string GrupoVinculado { get; set; } = string.Empty;
    public string RolEnGrupo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public string UniversidadSede { get; set; } = string.Empty;
}

/// <summary>
/// Consulta 7 (7 tablas: semillero + semillero_linea + linea_investigacion + linea_area + area_conocimiento + linea_ods + objetivo_desarrollo_sostenible).
/// Matriz de pertinencia temática de semilleros con líneas, disciplinas OCDE y ODS.
/// </summary>
public class Consulta7MatrizSemilleroDto
{
    public int SemilleroId { get; set; }
    public string SemilleroNombre { get; set; } = string.Empty;
    public string LineaInvestigacion { get; set; } = string.Empty;
    public string GranAreaOcde { get; set; } = string.Empty;
    public string DisciplinaOcde { get; set; } = string.Empty;
    public string OdsAsociado { get; set; } = string.Empty;
    public string CategoriaOds { get; set; } = string.Empty;
}

/// <summary>
/// Consulta 8 (5 tablas: universidad + grupo_investigacion + docente + semillero + docente_grupo).
/// Capacidad investigativa agregada por universidad y ciudad.
/// </summary>
public class Consulta8CapacidadUniversidadDto
{
    public int UniversidadId { get; set; }
    public string UniversidadNombre { get; set; } = string.Empty;
    public string TipoInstitucion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public int TotalGrupos { get; set; }
    public int TotalSemilleros { get; set; }
    public int TotalLideres { get; set; }
    public int TotalInvestigadoresVinculados { get; set; }
}

/// <summary>
/// Consulta 9 (6 tablas: linea_investigacion + grupo_linea + semillero_linea + linea_ods + linea_area + linea_area_aplicacion).
/// Mapa integral 360° de cada línea de investigación y su densidad de articulación.
/// </summary>
public class Consulta9MapaIntegralLineaDto
{
    public int LineaId { get; set; }
    public string LineaNombre { get; set; } = string.Empty;
    public int GruposAsociados { get; set; }
    public int SemillerosAsociados { get; set; }
    public int AreasOcdeAsociadas { get; set; }
    public int OdsAsociados { get; set; }
    public int AplicacionesAsociadas { get; set; }
}

/// <summary>
/// Consulta 10 (4 tablas: usuario + rol_usuario + rol + docente).
/// Auditoría de seguridad RBAC cruzando cuentas de usuario, roles asignados y directorio docente.
/// </summary>
public class Consulta10AuditoriaRbacDto
{
    public int UsuarioId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string RolAsignado { get; set; } = string.Empty;
    public string DescripcionRol { get; set; } = string.Empty;
    public string PerfilDocenteCruzado { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
}

/// <summary>
/// DTO Maestro que consolida las 10 consultas multitabla para el Dashboard Ejecutivo (v0.4).
/// </summary>
public class DashboardEjecutivoDto
{
    public IEnumerable<Consulta1GrupoProductividadDto> Consulta1GruposProductividad { get; set; } = Array.Empty<Consulta1GrupoProductividadDto>();
    public IEnumerable<Consulta2SemilleroTutorDto> Consulta2SemillerosTutores { get; set; } = Array.Empty<Consulta2SemilleroTutorDto>();
    public IEnumerable<Consulta3ImpactoOdsDto> Consulta3ImpactoOds { get; set; } = Array.Empty<Consulta3ImpactoOdsDto>();
    public IEnumerable<Consulta4CoberturaOcdeDto> Consulta4CoberturaOcde { get; set; } = Array.Empty<Consulta4CoberturaOcdeDto>();
    public IEnumerable<Consulta5TransferenciaAplicacionDto> Consulta5TransferenciaAplicacion { get; set; } = Array.Empty<Consulta5TransferenciaAplicacionDto>();
    public IEnumerable<Consulta6TrayectoriaDocenteDto> Consulta6TrayectoriaDocentes { get; set; } = Array.Empty<Consulta6TrayectoriaDocenteDto>();
    public IEnumerable<Consulta7MatrizSemilleroDto> Consulta7MatrizSemilleros { get; set; } = Array.Empty<Consulta7MatrizSemilleroDto>();
    public IEnumerable<Consulta8CapacidadUniversidadDto> Consulta8CapacidadUniversidades { get; set; } = Array.Empty<Consulta8CapacidadUniversidadDto>();
    public IEnumerable<Consulta9MapaIntegralLineaDto> Consulta9MapaIntegralLineas { get; set; } = Array.Empty<Consulta9MapaIntegralLineaDto>();
    public IEnumerable<Consulta10AuditoriaRbacDto> Consulta10AuditoriaRbac { get; set; } = Array.Empty<Consulta10AuditoriaRbacDto>();
}
