using System.Text.Json.Nodes;
using MIRA.Api.Modelos;

namespace MIRA.Api.Repositorios.Investigacion;

public sealed partial class InvestigacionRepository
{
    // Solo atributos descriptivos de referencias académicas; nunca hashes/cuentas.
    private const string ReferenciasSql = """
        jsonb_build_object(
        'sedes',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',nombre,'activo',activa) ORDER BY nombre,id),'[]'::jsonb) FROM public.sedes),
        'referencias/usuarios',public.sp_referencias_v2_usuarios(),
        'docentes',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',usuario_nombre) ORDER BY usuario_nombre,id),'[]'::jsonb) FROM public.v2_docentes),
        'grupos_investigacion',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',etiqueta,'activo',activo) ORDER BY etiqueta,id),'[]'::jsonb) FROM public.v2_grupos_investigacion),
        'semilleros',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',nombre,'activo',activo) ORDER BY nombre,id),'[]'::jsonb) FROM public.semilleros),
        'lineas_investigacion',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',codigo||' · '||nombre,'activo',activo) ORDER BY codigo,id),'[]'::jsonb) FROM public.lineas_investigacion),
        'areas_conocimiento',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',nombre,'activo',activo) ORDER BY nombre,id),'[]'::jsonb) FROM public.areas_conocimiento),
        'ods',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',nombre,'activo',activo) ORDER BY nombre,id),'[]'::jsonb) FROM public.ods),
        'areas_aplicacion',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',nombre,'activo',activo) ORDER BY nombre,id),'[]'::jsonb) FROM public.areas_aplicacion))
        """;

    private static readonly Dictionary<string,(string Resource,string ForeignKey)[]> Dependencias = new()
    {
        ["sedes"]=[("grupos_investigacion","sede_id")],
        ["docentes"]=[("grupo_docentes","docente_id"),("semillero_tutores","docente_id")],
        ["grupos_investigacion"]=[("semilleros","grupo_id"),("grupo_docentes","grupo_id"),("grupo_lineas","grupo_id")],
        ["semilleros"]=[("semillero_tutores","semillero_id"),("semillero_lineas","semillero_id")],
        ["lineas_investigacion"]=[("linea_disciplinas","linea_id"),("linea_ods","linea_id"),("linea_aplicacion","linea_id"),("grupo_lineas","linea_id"),("semillero_lineas","linea_id")],
        ["areas_conocimiento"]=[("linea_disciplinas","disciplina_id")],
        ["ods"]=[("linea_ods","ods_id")],
        ["areas_aplicacion"]=[("linea_aplicacion","aplicacion_id")]
    };

    public Task<JsonNode?> ReferenciasDescriptivas() => Consultar($"SELECT {ReferenciasSql};");

    public Task<JsonNode?> Detalle(RecursoInvestigacion recurso,int id)
    {
        var name=Nombre(recurso);
        if(recurso.EsPuente) throw new InvalidOperationException("La asociación requiere sus dos claves.");
        var related=Dependencias.GetValueOrDefault(name,[]);
        // Vistas/FK provienen de metadatos cerrados, no del cliente. Una sola
        // sentencia proporciona una fotografía consistente del maestro y detalles.
        var sections=related.Select(link => {
            var view=RecursosInvestigacion.Buscar(link.Resource)!.EsPuente ? link.Resource : "v2_"+link.Resource;
            return $"'{link.Resource}',(SELECT coalesce(jsonb_agg(to_jsonb(h) ORDER BY to_jsonb(h)::text),'[]'::jsonb) FROM public.{view} h WHERE h.{link.ForeignKey}=@Id)";
        });
        var relations=related.Length==0 ? "'{}'::jsonb" : "jsonb_build_object("+string.Join(',',sections)+")";
        return Consultar($"WITH principal AS (SELECT (SELECT to_jsonb(t) FROM public.v2_{name} t WHERE t.id=@Id) AS dato) SELECT CASE WHEN dato IS NULL THEN NULL ELSE jsonb_build_object('registro',dato,'relaciones',{relations},'referencias',{ReferenciasSql}) END FROM principal;",new {Id=id});
    }
}
