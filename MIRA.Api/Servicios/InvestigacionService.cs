using System.Globalization;
using System.Text.Json.Nodes;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios.Investigacion;

namespace MIRA.Api.Servicios;

public sealed class ValidacionInvestigacionException(string mensaje) : Exception(mensaje);
public sealed class RecursoInvestigacionNoEncontradoException() : Exception;

public sealed class InvestigacionService(InvestigacionRepository repository)
{
    public RecursoInvestigacion Recurso(string nombre, bool? puente = null)
    {
        var recurso = RecursosInvestigacion.Buscar(nombre);
        if (recurso is null || (puente.HasValue && recurso.EsPuente != puente))
            throw new RecursoInvestigacionNoEncontradoException();
        return recurso;
    }

    public JsonObject Validar(RecursoInvestigacion recurso, JsonNode? entrada)
    {
        if (entrada is not JsonObject datos) throw new ValidacionInvestigacionException("Los datos del formulario no son válidos.");
        if (datos.Any(pair => !recurso.Campos.Any(c => c.Nombre == pair.Key)))
            throw new ValidacionInvestigacionException("El formulario contiene campos que no se pueden modificar.");
        var result = new JsonObject();
        foreach (var campo in recurso.Campos)
        {
            var valor = datos[campo.Nombre];
            var etiqueta = campo.Nombre.Replace('_',' ');
            if (valor is null || (valor is JsonValue j && j.TryGetValue<string>(out var vacio) && string.IsNullOrWhiteSpace(vacio)))
            {
                if (!campo.Opcional) throw new ValidacionInvestigacionException($"El campo {etiqueta} es obligatorio.");
                result[campo.Nombre] = null;
                continue;
            }
            if (campo.Tipo == "referencias")
            {
                if(valor is not JsonArray ids || ids.Count is <1 or >200)
                    throw new ValidacionInvestigacionException($"Seleccione entre una y doscientas referencias para {etiqueta}.");
                var claves=new List<int>();
                foreach(var item in ids)
                {
                    if(item is not JsonValue n || !n.TryGetValue<int>(out var key) || key<=0)
                        throw new ValidacionInvestigacionException($"Seleccione referencias válidas para {etiqueta}.");
                    claves.Add(key);
                }
                if(claves.Distinct().Count()!=claves.Count) throw new ValidacionInvestigacionException($"No repita referencias en {etiqueta}.");
                result[campo.Nombre]=new JsonArray(claves.Order().Select(key=>(JsonNode?)JsonValue.Create(key)).ToArray());
                continue;
            }
            if (campo.Tipo == "referencia")
            {
                if (valor is not JsonValue numero || !numero.TryGetValue<int>(out var id) || id <= 0)
                    throw new ValidacionInvestigacionException($"Seleccione una referencia válida para {etiqueta}.");
                result[campo.Nombre] = id;
                continue;
            }
            if (valor is not JsonValue texto || !texto.TryGetValue<string>(out var cadena))
                throw new ValidacionInvestigacionException($"El campo {etiqueta} debe contener texto.");
            cadena = cadena.Trim();
            if (campo.Maximo > 0 && cadena.Length > campo.Maximo)
                throw new ValidacionInvestigacionException($"El campo {etiqueta} admite hasta {campo.Maximo} caracteres.");
            if (campo.Opciones is not null && !campo.Opciones.Contains(cadena))
                throw new ValidacionInvestigacionException($"Seleccione una opción válida para {etiqueta}.");
            if (campo.Tipo == "fecha" && !DateOnly.TryParseExact(cadena,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _))
                throw new ValidacionInvestigacionException($"El campo {etiqueta} debe ser una fecha válida.");
            if (campo.Tipo == "url" && (!Uri.TryCreate(cadena,UriKind.Absolute,out var uri) || (uri.Scheme != "http" && uri.Scheme != "https")))
                throw new ValidacionInvestigacionException($"El campo {etiqueta} debe ser un enlace http o https válido.");
            result[campo.Nombre] = cadena;
        }
        if (result["fecha_fin"] is not null && result["fecha_inicio"] is not null &&
            string.CompareOrdinal(result["fecha_fin"]!.GetValue<string>(),result["fecha_inicio"]!.GetValue<string>()) <= 0)
            throw new ValidacionInvestigacionException("La fecha final debe ser posterior a la fecha inicial.");
        return result;
    }

    public Task<JsonNode?> Listar(string recurso) => repository.Listar(Recurso(recurso));
    public Task<JsonNode?> Obtener(string recurso, int id) => repository.Obtener(Recurso(recurso,false),id);
    public Task<JsonNode?> ObtenerPuente(string recurso, int izquierda, int derecha) => repository.ObtenerPuente(Recurso(recurso,true),izquierda,derecha);
    public Task<JsonNode?> Crear(string nombre, JsonNode? datos)
    {
        var recurso = Recurso(nombre);
        return repository.Crear(recurso,Validar(recurso,datos));
    }
    public Task<JsonNode?> Actualizar(string nombre, int id, JsonNode? datos)
    {
        var recurso = Recurso(nombre,false);
        return repository.Actualizar(recurso,id,Validar(recurso,datos));
    }
    public Task<bool> Eliminar(string nombre, int id, int? derecha = null) => repository.Eliminar(Recurso(nombre,derecha.HasValue),id,derecha);
    public Task<JsonNode?> Usuarios() => repository.Usuarios();
    public Task<JsonNode?> Ficha(int id) => repository.Ficha(id);
    private static void AsociacionDeLineas(string recurso)
    {
        if (recurso is not ("grupo_lineas" or "semillero_lineas")) throw new RecursoInvestigacionNoEncontradoException();
    }
    public Task<JsonNode?> AsignarLineas(string recurso,AsignarLineasRequest datos)
    {
        AsociacionDeLineas(recurso);
        if(datos.PadreId<=0 || datos.Lineas is null || datos.Lineas.Length is <1 or >200 || datos.Lineas.Any(id=>id<=0) || datos.Lineas.Distinct().Count()!=datos.Lineas.Length)
            throw new ValidacionInvestigacionException("Seleccione un registro y entre una y doscientas líneas distintas.");
        return repository.AsignarLineas(recurso,datos.PadreId,datos.Lineas.Order().ToArray());
    }
    public Task<JsonNode?> ResumenLineas(string recurso,int padre)
    {
        AsociacionDeLineas(recurso);
        return repository.ResumenLineas(recurso,padre);
    }
    public Task<JsonNode?> FiltrarSemilleros(int? grupo,int? docente)
    {
        if(grupo is <=0 || docente is <=0) throw new ValidacionInvestigacionException("Seleccione referencias válidas para filtrar semilleros.");
        return repository.FiltrarSemilleros(grupo,docente);
    }
    public Task<JsonNode?> OpcionesReportes() => repository.OpcionesReportes();
    public Task<JsonNode?> LineasInactivas() => repository.LineasInactivas();
    public Task<JsonNode?> EstadoLinea(int id,EstadoLineaRequest request)
    {
        if(request.Activo is null) throw new ValidacionInvestigacionException("Indique explícitamente el estado de la línea.");
        return repository.EstadoLinea(id,request.Activo.Value);
    }
    public Task<JsonNode?> ReporteIntegrantes(int grupo) => repository.ReporteIntegrantes(grupo);
    public Task<JsonNode?> ReporteArticulacion(int? sede,int? linea)
    {
        if(sede is <=0 || linea is <=0) throw new ValidacionInvestigacionException("Seleccione referencias válidas para filtrar el reporte.");
        return repository.ReporteArticulacion(sede,linea);
    }
    public Task<JsonNode?> GuardarFicha(int? id, JsonNode? entrada)
    {
        if (entrada is not JsonObject datos || datos["semilleros"] is not JsonArray detalles ||
            datos.Any(p => p.Key is not ("grupo" or "semilleros")))
            throw new ValidacionInvestigacionException("La ficha debe contener un grupo y su lista de semilleros.");
        var grupo = Validar(Recurso("grupos_investigacion"),datos["grupo"]);
        if (detalles.Count > 200) throw new ValidacionInvestigacionException("Guarde como máximo 200 semilleros por ficha.");
        var filas = new JsonArray();
        foreach (var detalle in detalles)
        {
            if (detalle is not JsonObject fila || fila.Any(p => p.Key is not ("id" or "nombre" or "descripcion")))
                throw new ValidacionInvestigacionException("Un renglón de semillero contiene campos inválidos.");
            var temporal = (JsonObject)fila.DeepClone();
            var clave = temporal["id"]?.DeepClone();
            temporal.Remove("id");
            // The parent comes from the sheet context, never from a detail row.
            var camposDetalle = Recurso("semilleros") with { Campos=Recurso("semilleros").Campos.Where(c=>c.Nombre!="grupo_id").ToArray() };
            var validado = Validar(camposDetalle,temporal);
            if (clave is not null)
            {
                if (clave is not JsonValue numero || !numero.TryGetValue<int>(out var childId) || childId <= 0 || id is null)
                    throw new ValidacionInvestigacionException("El identificador de un semillero no es válido para esta ficha.");
                validado["id"] = childId;
            }
            filas.Add(validado);
        }
        return repository.GuardarFicha(id,new JsonObject { ["grupo"]=grupo,["semilleros"]=filas });
    }
}
