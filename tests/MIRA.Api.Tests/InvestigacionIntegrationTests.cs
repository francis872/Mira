using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Npgsql;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class InvestigacionIntegrationTests
{
    private const string Prefix = "/api/investigacion/";
    private static async Task<JsonObject> Create(HttpClient client, string resource, object payload)
    {
        var response = await client.PostAsJsonAsync(Prefix+resource,payload);
        Assert.True(response.StatusCode == HttpStatusCode.Created,$"{resource}: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        return (await response.Content.ReadFromJsonAsync<JsonObject>())!;
    }
    private static int Id(JsonObject item) => item["id"]!.GetValue<int>();
    private static async Task<int> Person()
    {
        await using var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("MIRA_TEST_DB"));
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("INSERT INTO usuarios(nombre,correo,password_hash) VALUES ('Persona prueba API', @correo, 'NO_ES_UNA_CREDENCIAL') RETURNING id",connection);
        command.Parameters.AddWithValue("correo",Guid.NewGuid()+"@invalid.test");
        return (int)(await command.ExecuteScalarAsync())!;
    }
    private static async Task<(int Sede,int Grupo,int Semillero,int Docente,int Linea)> References(HttpClient client)
    {
        var sede=Id(await Create(client,"sedes",new { nombre="Sede prueba API",ciudad="Medellín" }));
        var grupo=Id(await Create(client,"grupos_investigacion",new { sede_id=sede,gruptac_url="https://example.org/grupo" }));
        var semillero=Id(await Create(client,"semilleros",new { grupo_id=grupo,nombre="Semillero prueba API" }));
        var docente=Id(await Create(client,"docentes",new { usuario_id=await Person() }));
        var linea=Id(await Create(client,"lineas_investigacion",new { codigo=Guid.NewGuid().ToString("N"),nombre="Línea prueba API" }));
        return (sede,grupo,semillero,docente,linea);
    }

    [RequiresDatabaseFact]
    public async Task ElevenResources_AllFiveOperationsPersistAndFilterRemovedRows()
    {
        using var factory=TestHostFactory.Create();
        using var client=factory.CreateClient();
        var refs=await References(client);
        var person=await Person();
        var cases=new (string Resource,object Data)[] {
            ("sedes",new { nombre="Nueva sede",ciudad="Ciudad" }),
            ("docentes",new { usuario_id=person,cvlac_url=(string?)null }),
            ("grupos_investigacion",new { sede_id=refs.Sede,gruptac_url="https://example.org/nuevo",categoria=(string?)null }),
            ("semilleros",new { grupo_id=refs.Grupo,nombre="Nuevo semillero",descripcion=(string?)null }),
            ("lineas_investigacion",new { codigo=Guid.NewGuid().ToString("N"),nombre="Nueva línea" }),
            ("grupo_docentes",new { grupo_id=refs.Grupo,docente_id=refs.Docente,rol="Coinvestigador",fecha_inicio="2026-01-01",fecha_fin=(string?)null }),
            ("semillero_tutores",new { semillero_id=refs.Semillero,docente_id=refs.Docente,rol="Tutor",fecha_inicio="2026-01-01",fecha_fin=(string?)null }),
            ("areas_conocimiento",new { nombre=Guid.NewGuid().ToString() }),
            ("ods",new { nombre=Guid.NewGuid().ToString() }),
            ("areas_aplicacion",new { nombre=Guid.NewGuid().ToString() }),
            ("palabras_clave",new { nombre=Guid.NewGuid().ToString() })
        };
        foreach (var (resource,data) in cases)
        {
            var item=await Create(client,resource,data);
            var id=Id(item);
            Assert.NotNull(item["created_at"] ?? item["activo"]); // Stored defaults, not an echoed input object.
            var location=Prefix+resource+"/"+id;
            Assert.Equal(HttpStatusCode.OK,(await client.GetAsync(location)).StatusCode);
            var list=(await client.GetFromJsonAsync<JsonArray>(Prefix+resource))!;
            Assert.Contains(list,n=>n!["id"]!.GetValue<int>()==id);
            Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync(location,data)).StatusCode);
            Assert.Equal(HttpStatusCode.OK,(await client.DeleteAsync(location)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync(location)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound,(await client.PutAsJsonAsync(location,data)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound,(await client.DeleteAsync(location)).StatusCode);
            list=(await client.GetFromJsonAsync<JsonArray>(Prefix+resource))!;
            Assert.DoesNotContain(list,n=>n!["id"]!.GetValue<int>()==id);
        }
    }

    [RequiresDatabaseFact]
    public async Task AllFiveBridges_AssignQueryDuplicateConflictAndRemove()
    {
        using var factory=TestHostFactory.Create();
        using var client=factory.CreateClient();
        var refs=await References(client);
        var disciplina=Id(await Create(client,"areas_conocimiento",new { nombre=Guid.NewGuid().ToString() }));
        var ods=Id(await Create(client,"ods",new { nombre=Guid.NewGuid().ToString() }));
        var app=Id(await Create(client,"areas_aplicacion",new { nombre=Guid.NewGuid().ToString() }));
        var cases=new (string Resource,string Left,int A,string Right,int B)[] {
            ("grupo_lineas","grupo_id",refs.Grupo,"linea_id",refs.Linea),
            ("semillero_lineas","semillero_id",refs.Semillero,"linea_id",refs.Linea),
            ("linea_disciplinas","linea_id",refs.Linea,"disciplina_id",disciplina),
            ("linea_ods","linea_id",refs.Linea,"ods_id",ods),
            ("linea_aplicacion","linea_id",refs.Linea,"aplicacion_id",app)
        };
        foreach(var (resource,left,a,right,b) in cases)
        {
            var data=new JsonObject { [left]=a,[right]=b };
            await Create(client,resource,data);
            Assert.Equal(HttpStatusCode.Conflict,(await client.PostAsJsonAsync(Prefix+resource,data)).StatusCode);
            Assert.Equal(HttpStatusCode.OK,(await client.GetAsync($"{Prefix}{resource}/{a}/{b}")).StatusCode);
            Assert.Equal(HttpStatusCode.OK,(await client.DeleteAsync($"{Prefix}{resource}/{a}/{b}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync($"{Prefix}{resource}/{a}/{b}")).StatusCode);
        }
    }

    [RequiresDatabaseFact]
    public async Task InvalidReferenceAndInactiveParentTrigger_ReturnConflictWithoutInternals()
    {
        using var factory=TestHostFactory.Create();
        using var client=factory.CreateClient();
        var response=await client.PostAsJsonAsync(Prefix+"grupos_investigacion",new { sede_id=2147483647,gruptac_url="https://example.org/invalid" });
        Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
        var body=await response.Content.ReadAsStringAsync();
        Assert.Contains("referencias",body);
        Assert.DoesNotContain("23503",body);
        var refs=await References(client);
        await client.DeleteAsync(Prefix+"grupos_investigacion/"+refs.Grupo);
        response=await client.PostAsJsonAsync(Prefix+"semilleros",new { grupo_id=refs.Grupo,nombre="Invalid parent" });
        Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
    }

    [RequiresDatabaseFact]
    public async Task SheetCreatesManyRowsThenRollsBackAllChangesWhenLastRowBelongsElsewhere()
    {
        using var factory=TestHostFactory.Create();
        using var client=factory.CreateClient();
        var sede=Id(await Create(client,"sedes",new { nombre="Ficha prueba API",ciudad="Medellín" }));
        var group=new { sede_id=sede,gruptac_url="https://example.org/atomic",categoria=(string?)null };
        var response=await client.PostAsJsonAsync(Prefix+"grupos/ficha",new { grupo=group,semilleros=new[]{new{nombre="Primero",descripcion=(string?)null},new{nombre="Segundo",descripcion=(string?)null}} });
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        var sheet=(await response.Content.ReadFromJsonAsync<JsonObject>())!;
        var id=sheet["grupo"]!["id"]!.GetValue<int>();
        var child=sheet["semilleros"]![0]!["id"]!.GetValue<int>();
        var bad=new { grupo=new { sede_id=sede,gruptac_url="https://example.org/changed" },semilleros=new[]{new{id=child,nombre="Changed"},new{id=2147483647,nombre="Invalid"}} };
        Assert.Equal(HttpStatusCode.Conflict,(await client.PutAsJsonAsync(Prefix+"grupos/ficha/"+id,bad)).StatusCode);
        var after=(await client.GetFromJsonAsync<JsonObject>(Prefix+"grupos/ficha/"+id))!;
        Assert.True(JsonNode.DeepEquals(sheet,after),"The successful earlier detail and master writes must both be reverted");
        var good=new { grupo=group,semilleros=new[]{new{id=child,nombre="Editado",descripcion=(string?)null}} };
        Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync(Prefix+"grupos/ficha/"+id,good)).StatusCode);
        after=(await client.GetFromJsonAsync<JsonObject>(Prefix+"grupos/ficha/"+id))!;
        Assert.Single(after["semilleros"]!.AsArray());
        Assert.Equal("Editado",after["semilleros"]![0]!["nombre"]!.GetValue<string>());
    }

    [RequiresDatabaseFact]
    public async Task NullsDatesAndEnumerationsAreValidated_AndPersonReferenceDoesNotExposeCredentials()
    {
        using var factory=TestHostFactory.Create();
        using var client=factory.CreateClient();
        var refs=await References(client);
        var docente=(await client.GetFromJsonAsync<JsonObject>(Prefix+"docentes/"+refs.Docente))!;
        Assert.Null(docente["cvlac_url"]);
        Assert.Null(docente["minciencias_categoria"]);
        var response=await client.PostAsJsonAsync(Prefix+"semillero_tutores",new { semillero_id=refs.Semillero,docente_id=refs.Docente,rol="Tutor",fecha_inicio="2026-02-01",fecha_fin="2026-01-01" });
        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
        response=await client.PostAsJsonAsync(Prefix+"docentes",new { usuario_id=await Person(),minciencias_categoria="Otro" });
        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
        var people=(await client.GetFromJsonAsync<JsonArray>(Prefix+"referencias/usuarios"))!;
        Assert.All(people,p=>Assert.Equal(new[]{"id","nombre"},p!.AsObject().Select(x=>x.Key).Order().ToArray()));
    }

    [RequiresDatabaseFact]
    public async Task UnknownEntitiesAndWritableFieldsAreRejected()
    {
        using var factory=TestHostFactory.Create();
        using var client=factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync(Prefix+"usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await client.PostAsJsonAsync(Prefix+"usuarios",new{})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync(Prefix+"sedes",new{nombre="Sede",ciudad="Ciudad",activa=false})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync(Prefix+"semilleros",new{grupo_id="1",nombre="No numeric strings"})).StatusCode);
    }
}
