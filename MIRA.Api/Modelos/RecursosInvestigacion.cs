namespace MIRA.Api.Modelos;

public sealed record CampoInvestigacion(string Nombre, string Tipo = "texto", int Maximo = 0,
    bool Opcional = false, string[]? Opciones = null);

public sealed record RecursoInvestigacion(string Nombre, CampoInvestigacion[] Campos, string[]? Claves = null)
{
    public bool EsPuente => Claves is not null;
}

/// <summary>Closed list matching the columns of the confirmed SQL; never populated from request data.</summary>
public static class RecursosInvestigacion
{
    private static CampoInvestigacion Texto(string nombre, int maximo, bool opcional = false) => new(nombre, "texto", maximo, opcional);
    private static CampoInvestigacion Referencia(string nombre) => new(nombre, "referencia");
    private static CampoInvestigacion Fecha(string nombre, bool opcional = false) => new(nombre, "fecha", Opcional: opcional);
    private static readonly RecursoInvestigacion[] Recursos =
    [
        new("sedes", [Texto("nombre",120), Texto("ciudad",120)]),
        new("docentes", [Referencia("usuario_id"), new("cvlac_url","url",500,true), Texto("escalafon",100,true),
            new("minciencias_categoria","texto",50,true,["Senior","Asociado","Junior","Sin Categoría"])]),
        new("grupos_investigacion", [Referencia("sede_id"), new("gruptac_url","url",500), Texto("categoria",100,true)]),
        new("semilleros", [Referencia("grupo_id"), Texto("nombre",200), Texto("descripcion",0,true)]),
        new("lineas_investigacion", [Texto("codigo",50), Texto("nombre",200), Texto("descripcion",0,true)]),
        new("grupo_docentes", [Referencia("grupo_id"), Referencia("docente_id"),
            new("rol",Opciones:["Líder de Grupo","Coinvestigador","Colaborador"]), Fecha("fecha_inicio"), Fecha("fecha_fin",true)]),
        new("semillero_tutores", [Referencia("semillero_id"), Referencia("docente_id"),
            new("rol",Opciones:["Director","Tutor","Coinvestigador"]), Fecha("fecha_inicio"), Fecha("fecha_fin",true)]),
        new("areas_conocimiento", [Texto("nombre",160)]),
        new("ods", [Texto("nombre",160)]),
        new("areas_aplicacion", [Texto("nombre",160)]),
        new("palabras_clave", [Texto("nombre",160)]),
        Puente("grupo_lineas","grupo_id","linea_id"),
        Puente("semillero_lineas","semillero_id","linea_id"),
        Puente("linea_disciplinas","linea_id","disciplina_id"),
        Puente("linea_ods","linea_id","ods_id"),
        Puente("linea_aplicacion","linea_id","aplicacion_id")
    ];
    private static RecursoInvestigacion Puente(string nombre, string izquierda, string derecha) =>
        new(nombre,[Referencia(izquierda),Referencia(derecha)],[izquierda,derecha]);
    public static RecursoInvestigacion? Buscar(string nombre) => Recursos.FirstOrDefault(r => r.Nombre == nombre);
}
