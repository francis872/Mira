using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Repositorios.Seguridad;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class V2RequisitosOriginalesTests
{
    private const string P="/api/investigacion/";
    private static async Task<int> Create(HttpClient c,string resource,object data)
    {
        var r=await c.PostAsJsonAsync(P+resource,await ResearchTestData.WithLineLinks(c,resource,data));
        Assert.True(r.StatusCode==HttpStatusCode.Created,await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<int>();
    }
    private static Task<int> Sede(HttpClient c) => Create(c,"sedes",new {nombre="Prueba F5",ciudad="Medellín",codigo_institucional="PRUEBA-F5"});
    private static Task<int> Grupo(HttpClient c,int sede) => Create(c,"grupos_investigacion",new {sede_id=sede,gruptac_url="https://example.invalid/"+Guid.NewGuid()});
    private static Task<int> Linea(HttpClient c) => Create(c,"lineas_investigacion",new {codigo=Guid.NewGuid().ToString("N"),nombre="Línea F5",descripcion="Descripción académica"});

    [RequiresDatabaseTheory]
    [InlineData("sedes","codigo_institucional")]
    [InlineData("palabras_clave","termino_ingles")]
    [InlineData("lineas_investigacion","descripcion")]
    public async Task OriginalFieldsAreRequiredAndPersistWithoutChangingContractKeys(string resource,string field)
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var data=new JsonObject { ["nombre"]="f5-"+Guid.NewGuid() };
        if(resource=="sedes") data["ciudad"]="Prueba";
        if(resource=="lineas_investigacion") data["codigo"]=Guid.NewGuid().ToString("N");
        data=await ResearchTestData.WithLineLinks(c,resource,data);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsJsonAsync(P+resource,data)).StatusCode);
        data[field]="Contenido de prueba";
        var id=await Create(c,resource,data);
        var stored=await c.GetFromJsonAsync<JsonObject>(P+resource+"/"+id);
        Assert.Equal("Contenido de prueba",stored![field]!.GetValue<string>());
        data[field]="  ";
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PutAsJsonAsync(P+resource+"/"+id,data)).StatusCode);
        stored=await c.GetFromJsonAsync<JsonObject>(P+resource+"/"+id);
        Assert.Equal("Contenido de prueba",stored![field]!.GetValue<string>());
    }

    [RequiresDatabaseFact]
    public async Task InactiveSedePreservesOldGroupButRejectsNewAssignments()
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var sede=await Sede(c);var grupo=await Grupo(c,sede);
        Assert.Equal(HttpStatusCode.OK,(await c.DeleteAsync(P+"sedes/"+sede)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await c.GetAsync(P+"grupos_investigacion/"+grupo)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await c.PostAsJsonAsync(P+"grupos_investigacion",new {sede_id=sede,gruptac_url="https://example.invalid/nuevo"})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await c.PutAsJsonAsync(P+"grupos_investigacion/"+grupo,new {sede_id=sede,gruptac_url="https://example.invalid/historia"})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await c.PostAsJsonAsync(P+"grupos/ficha",new {grupo=new {sede_id=sede,gruptac_url="https://example.invalid/ficha"},semilleros=new[]{new {nombre="No debe persistir"}}})).StatusCode);
    }

    [RequiresDatabaseTheory]
    [InlineData("grupo_lineas")][InlineData("semillero_lineas")]
    public async Task MultipleLineAssignmentIsAtomicAndInactiveLinesRemainInSummary(string resource)
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var grupo=await Grupo(c,await Sede(c));
        var parent=resource=="grupo_lineas" ? grupo : await Create(c,"semilleros",new {grupo_id=grupo,nombre="Prueba F5"});
        var first=await Linea(c);var second=await Linea(c);
        var endpoint=P+"asociaciones/"+resource+"/asignar";
        Assert.Equal(HttpStatusCode.Conflict,(await c.PostAsJsonAsync(endpoint,new {padreId=parent,lineas=new[]{first,int.MaxValue}})).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await c.GetAsync($"{P}{resource}/{parent}/{first}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsJsonAsync(endpoint,new {padreId=parent,lineas=new[]{first,first}})).StatusCode);
        Assert.Equal(HttpStatusCode.Created,(await c.PostAsJsonAsync(endpoint,new {padreId=parent,lineas=new[]{first,second}})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await c.DeleteAsync(P+"lineas_investigacion/"+first)).StatusCode);
        var summary=(await c.GetFromJsonAsync<JsonArray>($"{P}asociaciones/{resource}/resumen/{parent}"))!;
        Assert.Equal(2,summary.Count);
        Assert.False(summary.Single(n=>n!["id"]!.GetValue<int>()==first)!["activo"]!.GetValue<bool>());
        var otherGroup=await Grupo(c,await Sede(c));
        var other=resource=="grupo_lineas" ? otherGroup : await Create(c,"semilleros",new {grupo_id=otherGroup,nombre="Otro F5"});
        Assert.Equal(HttpStatusCode.Conflict,(await c.PostAsJsonAsync(endpoint,new {padreId=other,lineas=new[]{first}})).StatusCode);
    }

    [RequiresDatabaseFact]
    public async Task SemilleroFiltersUseActualGroupAndTutorRelationships()
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var grupo=await Grupo(c,await Sede(c));
        var id=await Create(c,"semilleros",new {grupo_id=grupo,nombre="Filtrado F5"});
        var rows=(await c.GetFromJsonAsync<JsonArray>(P+"semilleros/buscar?grupoId="+grupo))!;
        Assert.Single(rows);Assert.Equal(id,rows[0]!["id"]!.GetValue<int>());
        rows=(await c.GetFromJsonAsync<JsonArray>(P+"semilleros/buscar?grupoId="+grupo+"&docenteId=2147483647"))!;
        Assert.Empty(rows);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.GetAsync(P+"semilleros/buscar?grupoId=0")).StatusCode);
    }

    [RequiresDatabaseTheory]
    [InlineData("semilleros/buscar")]
    [InlineData("asociaciones/grupo_lineas/resumen/1")]
    [InlineData("asociaciones/semillero_lineas/resumen/1")]
    public async Task NewReadRoutesRejectAnonymousClients(string path)
    {
        using var f=TestHostFactory.Create(authenticated:false);using var c=f.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,(await c.GetAsync(P+path)).StatusCode);
    }

    [RequiresDatabaseFact]
    public async Task ReadOnlyJwtCannotAssignLinesThroughNewEndpoint()
    {
        var repository=new SeguridadTests.CuentaStub();
        using var f=TestHostFactory.Create(s=>s.AddSingleton<ICuentaRepository>(repository),authenticated:false);using var c=f.CreateClient();
        var login=await c.PostAsJsonAsync("/api/auth/login",new {correo="test@invalid.test",password="Password-test-2026!"});
        Assert.Equal(HttpStatusCode.OK,login.StatusCode);
        c.DefaultRequestHeaders.Authorization=new("Bearer",(await login.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>());
        Assert.Equal(HttpStatusCode.Forbidden,(await c.PostAsJsonAsync(P+"asociaciones/grupo_lineas/asignar",new {padreId=1,lineas=new[]{1}})).StatusCode);
    }
}
