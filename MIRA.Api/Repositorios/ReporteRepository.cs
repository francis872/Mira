using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MIRA.Api.Configuracion;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios;

/// <summary>
/// Repositorio de analítica institucional (Versión 4).
/// Ejecuta las 10 consultas multitabla (de 4 a 7 tablas cada una) manteniendo el "SQL a la vista" con Dapper.
/// </summary>
public class ReporteRepository : IReporteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReporteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // -------------------------------------------------------------------------
    // CONSULTA 1 (5 Tablas): grupo_investigacion + universidad + docente + grupo_linea + linea_investigacion
    // Por qué usa LEFT JOIN hacia grupo_linea: un grupo recién creado puede aún no tener líneas vinculadas
    // en la tabla puente, y si fuera INNER JOIN desaparecería del reporte de productividad.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta1GrupoProductividadDto>> GetConsulta1GruposUniversidadLiderLineasAsync()
    {
        const string sql = @"
            SELECT g.id AS GrupoId,
                   g.codigo AS CodigoGrupo,
                   g.nombre AS GrupoNombre,
                   COALESCE(g.categoria_minciencias, 'Sin Categoría') AS CategoriaGrupo,
                   u.nombre AS UniversidadNombre,
                   u.ciudad AS Ciudad,
                   (d.nombres || ' ' || d.apellidos) AS DocenteLider,
                   d.correo AS CorreoLider,
                   COUNT(DISTINCT l.id) AS TotalLineas,
                   COALESCE(STRING_AGG(DISTINCT l.nombre, ' | '), 'Sin líneas articuladas') AS LineasArticuladas
            FROM grupo_investigacion g
            INNER JOIN universidad u ON g.universidad_id = u.id AND u.activo = TRUE
            INNER JOIN docente d ON g.docente_lider_cedula = d.cedula AND d.activo = TRUE
            LEFT JOIN grupo_linea gl ON g.id = gl.grupo_id AND gl.activo = TRUE
            LEFT JOIN linea_investigacion l ON gl.linea_id = l.id AND l.activo = TRUE
            WHERE g.activo = TRUE
            GROUP BY g.id, g.codigo, g.nombre, g.categoria_minciencias, u.nombre, u.ciudad, d.nombres, d.apellidos, d.correo
            ORDER BY TotalLineas DESC, g.nombre ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta1GrupoProductividadDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 2 (5 Tablas): semillero + grupo_investigacion + universidad + docente_semillero + docente
    // Por qué cruza hasta universidad: un semillero solo guarda grupo_investigacion_id, por lo que para
    // saber a qué sede universitaria pertenece y quién es su tutor es necesario navegar las 5 tablas.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta2SemilleroTutorDto>> GetConsulta2SemillerosGruposTutoresAsync()
    {
        const string sql = @"
            SELECT s.id AS SemilleroId,
                   s.nombre AS SemilleroNombre,
                   g.nombre AS GrupoNombre,
                   g.codigo AS CodigoGrupo,
                   u.nombre AS UniversidadNombre,
                   COALESCE(d.nombres || ' ' || d.apellidos, 'Sin tutor asignado') AS DocenteTutor,
                   COALESCE(d.escalafon, 'N/A') AS EscalafonTutor,
                   COALESCE(ds.rol, 'Pendiente') AS RolTutor,
                   COALESCE(ds.periodo_academico, 'N/A') AS PeriodoAcademico
            FROM semillero s
            INNER JOIN grupo_investigacion g ON s.grupo_investigacion_id = g.id AND g.activo = TRUE
            INNER JOIN universidad u ON g.universidad_id = u.id AND u.activo = TRUE
            LEFT JOIN docente_semillero ds ON s.id = ds.semillero_id AND ds.activo = TRUE
            LEFT JOIN docente d ON ds.docente_cedula = d.cedula AND d.activo = TRUE
            WHERE s.activo = TRUE
            ORDER BY ds.periodo_academico DESC NULLS LAST, s.nombre ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta2SemilleroTutorDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 3 (6 Tablas): objetivo_desarrollo_sostenible + linea_ods + linea_investigacion +
    //                        grupo_linea + grupo_investigacion + universidad
    // Trazabilidad completa desde la Agenda 2030 (ODS) hasta la universidad y grupo que la ejecuta.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta3ImpactoOdsDto>> GetConsulta3ImpactoOdsGruposUniversidadAsync()
    {
        const string sql = @"
            SELECT o.id AS OdsId,
                   o.nombre AS OdsNombre,
                   o.categoria AS CategoriaOds,
                   COALESCE(l.nombre, 'Sin línea vinculada') AS LineaNombre,
                   COALESCE(g.nombre, 'Sin grupo vinculado') AS GrupoNombre,
                   COALESCE(u.nombre, 'N/A') AS UniversidadNombre,
                   COUNT(DISTINCT g.id) AS TotalGruposImpactados
            FROM objetivo_desarrollo_sostenible o
            LEFT JOIN linea_ods lo ON o.id = lo.ods_id AND lo.activo = TRUE
            LEFT JOIN linea_investigacion l ON lo.linea_id = l.id AND l.activo = TRUE
            LEFT JOIN grupo_linea gl ON l.id = gl.linea_id AND gl.activo = TRUE
            LEFT JOIN grupo_investigacion g ON gl.grupo_id = g.id AND g.activo = TRUE
            LEFT JOIN universidad u ON g.universidad_id = u.id AND u.activo = TRUE
            WHERE o.activo = TRUE
            GROUP BY o.id, o.nombre, o.categoria, l.nombre, g.nombre, u.nombre
            ORDER BY TotalGruposImpactados DESC, o.id ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta3ImpactoOdsDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 4 (6 Tablas): area_conocimiento + linea_area + linea_investigacion +
    //                        grupo_linea + grupo_investigacion + docente
    // Conecta la taxonomía OCDE con las líneas, grupos y docentes líderes responsables.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta4CoberturaOcdeDto>> GetConsulta4CoberturaOcdeGruposLiderAsync()
    {
        const string sql = @"
            SELECT ac.id AS AreaId,
                   ac.gran_area AS GranArea,
                   ac.area AS Area,
                   ac.disciplina AS Disciplina,
                   COALESCE(l.nombre, 'Sin línea asociada') AS LineaNombre,
                   COALESCE(g.nombre, 'Sin grupo asociado') AS GrupoNombre,
                   COALESCE(d.nombres || ' ' || d.apellidos, 'N/A') AS DocenteLider
            FROM area_conocimiento ac
            LEFT JOIN linea_area la ON ac.id = la.area_conocimiento_id AND la.activo = TRUE
            LEFT JOIN linea_investigacion l ON la.linea_id = l.id AND l.activo = TRUE
            LEFT JOIN grupo_linea gl ON l.id = gl.linea_id AND gl.activo = TRUE
            LEFT JOIN grupo_investigacion g ON gl.grupo_id = g.id AND g.activo = TRUE
            LEFT JOIN docente d ON g.docente_lider_cedula = d.cedula AND d.activo = TRUE
            WHERE ac.activo = TRUE
            ORDER BY ac.gran_area ASC, ac.disciplina ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta4CoberturaOcdeDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 5 (6 Tablas): area_aplicacion + linea_area_aplicacion + linea_investigacion +
    //                        semillero_linea + semillero + grupo_investigacion
    // Muestra cómo cada sector productivo de aplicación es atendido por líneas, semilleros y su grupo matriz.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta5TransferenciaAplicacionDto>> GetConsulta5TransferenciaAplicacionSemillerosAsync()
    {
        const string sql = @"
            SELECT aa.id AS AplicacionId,
                   aa.nombre AS AreaAplicacion,
                   COALESCE(l.nombre, 'Sin línea vinculada') AS LineaNombre,
                   COALESCE(s.nombre, 'Sin semillero vinculado') AS SemilleroNombre,
                   COALESCE(g.nombre, 'N/A') AS GrupoMatriz,
                   COALESCE(g.categoria_minciencias, 'N/A') AS CategoriaGrupo
            FROM area_aplicacion aa
            LEFT JOIN linea_area_aplicacion laa ON aa.id = laa.area_aplicacion_id AND laa.activo = TRUE
            LEFT JOIN linea_investigacion l ON laa.linea_id = l.id AND l.activo = TRUE
            LEFT JOIN semillero_linea sl ON l.id = sl.linea_id AND sl.activo = TRUE
            LEFT JOIN semillero s ON sl.semillero_id = s.id AND s.activo = TRUE
            LEFT JOIN grupo_investigacion g ON s.grupo_investigacion_id = g.id AND g.activo = TRUE
            WHERE aa.activo = TRUE
            ORDER BY aa.nombre ASC, l.nombre ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta5TransferenciaAplicacionDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 6 (5 Tablas): docente + linea_investigacion + docente_grupo + grupo_investigacion + universidad
    // Historial completo de participación de cada investigador en grupos y universidades.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta6TrayectoriaDocenteDto>> GetConsulta6TrayectoriaDocentesGruposAsync()
    {
        const string sql = @"
            SELECT d.cedula AS Cedula,
                   (d.nombres || ' ' || d.apellidos) AS DocenteNombre,
                   COALESCE(d.escalafon, 'Sin Escalafón') AS Escalafon,
                   COALESCE(d.categoria_minciencias, 'Sin Categoría') AS CategoriaMinciencias,
                   COALESCE(l.nombre, 'Sin Línea Matriz') AS LineaMatriz,
                   COALESCE(g.nombre, 'Sin Grupo Asignado') AS GrupoVinculado,
                   COALESCE(dg.rol, 'Investigador') AS RolEnGrupo,
                   COALESCE(dg.fecha_inicio, CURRENT_DATE) AS FechaInicio,
                   COALESCE(u.nombre, 'N/A') AS UniversidadSede
            FROM docente d
            LEFT JOIN linea_investigacion l ON d.linea_investigacion_principal = l.id AND l.activo = TRUE
            LEFT JOIN docente_grupo dg ON d.cedula = dg.docente_cedula AND dg.activo = TRUE
            LEFT JOIN grupo_investigacion g ON dg.grupo_id = g.id AND g.activo = TRUE
            LEFT JOIN universidad u ON g.universidad_id = u.id AND u.activo = TRUE
            WHERE d.activo = TRUE
            ORDER BY d.apellidos ASC, dg.fecha_inicio DESC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta6TrayectoriaDocenteDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 7 (7 Tablas): semillero + semillero_linea + linea_investigacion +
    //                        linea_area + area_conocimiento + linea_ods + objetivo_desarrollo_sostenible
    // Matriz de pertinencia académica (OCDE) y sostenible (ODS) para cada semillero de investigación.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta7MatrizSemilleroDto>> GetConsulta7MatrizSemillerosLineasOcdeOdsAsync()
    {
        const string sql = @"
            SELECT s.id AS SemilleroId,
                   s.nombre AS SemilleroNombre,
                   COALESCE(l.nombre, 'Sin línea asignada') AS LineaInvestigacion,
                   COALESCE(ac.gran_area, 'Sin área OCDE') AS GranAreaOcde,
                   COALESCE(ac.disciplina, 'N/A') AS DisciplinaOcde,
                   COALESCE(o.nombre, 'Sin ODS vinculado') AS OdsAsociado,
                   COALESCE(o.categoria, 'N/A') AS CategoriaOds
            FROM semillero s
            LEFT JOIN semillero_linea sl ON s.id = sl.semillero_id AND sl.activo = TRUE
            LEFT JOIN linea_investigacion l ON sl.linea_id = l.id AND l.activo = TRUE
            LEFT JOIN linea_area la ON l.id = la.linea_id AND la.activo = TRUE
            LEFT JOIN area_conocimiento ac ON la.area_conocimiento_id = ac.id AND ac.activo = TRUE
            LEFT JOIN linea_ods lo ON l.id = lo.linea_id AND lo.activo = TRUE
            LEFT JOIN objetivo_desarrollo_sostenible o ON lo.ods_id = o.id AND o.activo = TRUE
            WHERE s.activo = TRUE
            ORDER BY s.nombre ASC, l.nombre ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta7MatrizSemilleroDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 8 (5 Tablas): universidad + grupo_investigacion + docente + semillero + docente_grupo
    //Usa COUNT(DISTINCT ...) para evitar que el producto cartesiano entre semilleros y docente_grupo
    // infle artificialmente los totales por universidad.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta8CapacidadUniversidadDto>> GetConsulta8CapacidadCiudadUniversidadAsync()
    {
        const string sql = @"
            SELECT u.id AS UniversidadId,
                   u.nombre AS UniversidadNombre,
                   u.tipo AS TipoInstitucion,
                   u.ciudad AS Ciudad,
                   COUNT(DISTINCT g.id) AS TotalGrupos,
                   COUNT(DISTINCT s.id) AS TotalSemilleros,
                   COUNT(DISTINCT d.cedula) AS TotalLideres,
                   COUNT(DISTINCT dg.docente_cedula) AS TotalInvestigadoresVinculados
            FROM universidad u
            LEFT JOIN grupo_investigacion g ON u.id = g.universidad_id AND g.activo = TRUE
            LEFT JOIN docente d ON g.docente_lider_cedula = d.cedula AND d.activo = TRUE
            LEFT JOIN semillero s ON g.id = s.grupo_investigacion_id AND s.activo = TRUE
            LEFT JOIN docente_grupo dg ON g.id = dg.grupo_id AND dg.activo = TRUE
            WHERE u.activo = TRUE
            GROUP BY u.id, u.nombre, u.tipo, u.ciudad
            ORDER BY TotalGrupos DESC, TotalSemilleros DESC, u.nombre ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta8CapacidadUniversidadDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 9 (6 Tablas): linea_investigacion + grupo_linea + semillero_linea +
    //                        linea_ods + linea_area + linea_area_aplicacion
    // Radiografía 360° de densidad relacional por cada línea de investigación.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta9MapaIntegralLineaDto>> GetConsulta9MapaIntegralLineasAsync()
    {
        const string sql = @"
            SELECT l.id AS LineaId,
                   l.nombre AS LineaNombre,
                   COUNT(DISTINCT gl.grupo_id) AS GruposAsociados,
                   COUNT(DISTINCT sl.semillero_id) AS SemillerosAsociados,
                   COUNT(DISTINCT la.area_conocimiento_id) AS AreasOcdeAsociadas,
                   COUNT(DISTINCT lo.ods_id) AS OdsAsociados,
                   COUNT(DISTINCT laa.area_aplicacion_id) AS AplicacionesAsociadas
            FROM linea_investigacion l
            LEFT JOIN grupo_linea gl ON l.id = gl.linea_id AND gl.activo = TRUE
            LEFT JOIN semillero_linea sl ON l.id = sl.linea_id AND sl.activo = TRUE
            LEFT JOIN linea_area la ON l.id = la.linea_id AND la.activo = TRUE
            LEFT JOIN linea_ods lo ON l.id = lo.linea_id AND lo.activo = TRUE
            LEFT JOIN linea_area_aplicacion laa ON l.id = laa.linea_id AND laa.activo = TRUE
            WHERE l.activo = TRUE
            GROUP BY l.id, l.nombre
            ORDER BY GruposAsociados DESC, SemillerosAsociados DESC, l.id ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta9MapaIntegralLineaDto>(sql);
    }

    // -------------------------------------------------------------------------
    // CONSULTA 10 (4 Tablas): usuario + rol_usuario + rol + docente
    // Cruza las cuentas de acceso RBAC con sus roles y verifica si el correo institucional
    // corresponde a un investigador activo en la tabla docente.
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<Consulta10AuditoriaRbacDto>> GetConsulta10AuditoriaSeguridadRbacDocentesAsync()
    {
        const string sql = @"
            SELECT u.id AS UsuarioId,
                   u.username AS Username,
                   u.email AS Email,
                   COALESCE(u.nombre_completo, u.username) AS NombreCompleto,
                   r.nombre AS RolAsignado,
                   COALESCE(r.descripcion, '') AS DescripcionRol,
                   COALESCE(d.nombres || ' ' || d.apellidos || ' (C.C. ' || d.cedula || ')', 'Cuenta Administrativa / Externa') AS PerfilDocenteCruzado,
                   u.fecha_creacion AS FechaAsignacion
            FROM usuario u
            INNER JOIN rol_usuario ru ON u.id = ru.usuario_id AND ru.activo = TRUE
            INNER JOIN rol r ON ru.rol_id = r.id AND r.activo = TRUE
            LEFT JOIN docente d ON LOWER(u.email) = LOWER(d.correo) AND d.activo = TRUE
            WHERE u.activo = TRUE
            ORDER BY u.id ASC, r.id ASC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Consulta10AuditoriaRbacDto>(sql);
    }
}
