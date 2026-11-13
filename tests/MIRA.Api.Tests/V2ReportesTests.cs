using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Repositorios.Seguridad;
using Npgsql;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class V2ReportesTests
{
    private const string P="/api/investigacion/";
    private static async Task<int> Add(HttpClient c,string resource,object data)
    {
        var r=await c.PostAsJsonAsync(P+resource,await ResearchTestData.WithLineLinks(c,resource,data));Assert.True(r.StatusCode==HttpStatusCode.Created,await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<int>();
    }
    private static async Task<int> Teacher(HttpClient c)
    {
        await using var db=new NpgsqlConnection(Environment.GetEnvironmentVariable("MIRA_TEST_DB"));await db.OpenAsync();
        await using var command=new NpgsqlCommand("INSERT INTO usuarios(nombre,correo,password_hash) VALUES ('José Pérez prueba',@correo,'NO_ES_UNA_CREDENCIAL') RETURNING id",db);
        command.Parameters.AddWithValue("correo",Guid.NewGuid()+"@invalid.test");var person=(int)(await command.ExecuteScalarAsync())!;
        return await Add(c,"docentes",new {usuario_id=person});
    }
    [RequiresDatabaseFact]
    public async Task ReportUsesActualRelationshipsAndFiltersWithoutCredentials()
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var sede=await Add(c,"sedes",new {nombre="Sede reporte",ciudad="Prueba",codigo_institucional="PRUEBA-REP"});
        var group=await Add(c,"grupos_investigacion",new {sede_id=sede,gruptac_url="https://example.invalid/reporte"});
        var seed=await Add(c,"semilleros",new {grupo_id=group,nombre="Semillero reporte"});
        var line=await Add(c,"lineas_investigacion",new {codigo=Guid.NewGuid().ToString("N"),nombre="Línea reporte",descripcion="Descripción reporte"});
        await AddBridge(c,"grupo_lineas",new {grupo_id=group,linea_id=line});
        await AddBridge(c,"semillero_lineas",new {semillero_id=seed,linea_id=line});
        await Add(c,"grupo_docentes",new {grupo_id=group,docente_id=await Teacher(c),rol="Coinvestigador",fecha_inicio="2026-01-01"});
        await Add(c,"semillero_tutores",new {semillero_id=seed,docente_id=await Teacher(c),rol="Tutor",fecha_inicio="2026-02-01"});
        var response=await c.GetAsync(P+$"reportes/articulacion?sedeId={sede}&lineaId={line}");
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);var text=await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password",text,StringComparison.OrdinalIgnoreCase);Assert.DoesNotContain("correo",text,StringComparison.OrdinalIgnoreCase);
        var rows=(await response.Content.ReadFromJsonAsync<JsonArray>())!;Assert.Equal(2,rows.Count);
        Assert.Contains(rows,r=>r!["semillero_id"] is null && r["rol"]!.GetValue<string>()=="Coinvestigador");
        Assert.Contains(rows,r=>r!["semillero_id"]?.GetValue<int>()==seed && r["rol"]!.GetValue<string>()=="Tutor");
        Assert.All(rows,r=>Assert.Equal(line,r!["linea_id"]!.GetValue<int>()));
        Assert.All(rows,r=>Assert.NotNull(r!["ods"]));
        Assert.Empty((await c.GetFromJsonAsync<JsonArray>(P+$"reportes/articulacion?sedeId={sede}&lineaId=2147483647"))!);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.GetAsync(P+"reportes/articulacion?facultadId=1")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.GetAsync(P+"reportes/articulacion?sedeId=0")).StatusCode);
    }
    private static async Task AddBridge(HttpClient c,string resource,object data) =>
        Assert.Equal(HttpStatusCode.Created,(await c.PostAsJsonAsync(P+resource,data)).StatusCode);

    [RequiresDatabaseFact]
    public async Task MemberReportPreservesInactiveMembershipAndMissingGroupIs404()
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var sede=await Add(c,"sedes",new {nombre="Sede integrantes",ciudad="Prueba",codigo_institucional="PRUEBA-INT"});
        var group=await Add(c,"grupos_investigacion",new {sede_id=sede,gruptac_url="https://example.invalid/integrantes"});
        var member=await Add(c,"grupo_docentes",new {grupo_id=group,docente_id=await Teacher(c),rol="Colaborador",fecha_inicio="2026-01-01"});
        await c.DeleteAsync(P+"grupo_docentes/"+member);
        var result=(await c.GetFromJsonAsync<JsonObject>(P+"reportes/integrantes/"+group))!;
        Assert.Single(result["integrantes"]!.AsArray());Assert.False(result["integrantes"]![0]!["activo"]!.GetValue<bool>());
        Assert.Equal(HttpStatusCode.NotFound,(await c.GetAsync(P+"reportes/integrantes/2147483647")).StatusCode);
    }

    [RequiresDatabaseTheory]
    [InlineData("reportes/opciones")][InlineData("reportes/articulacion")][InlineData("reportes/integrantes/1")]
    public async Task ReportEndpointsAreNotPublic(string path)
    {
        using var f=TestHostFactory.Create(authenticated:false);using var c=f.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,(await c.GetAsync(P+path)).StatusCode);
    }
    [RequiresDatabaseTheory]
    [InlineData("Investigador")][InlineData("Coordinador")][InlineData("Administrador")]
    public async Task ExistingReadRolesCanConsultReportsUsingRealJwt(string role)
    {
        var repo=new SeguridadTests.CuentaStub {RolesActuales=[role]};
        using var f=TestHostFactory.Create(s=>s.AddSingleton<ICuentaRepository>(repo),authenticated:false);using var c=f.CreateClient();
        var login=await c.PostAsJsonAsync("/api/auth/login",new {correo="test@invalid.test",password="Password-test-2026!"});
        Assert.Equal(HttpStatusCode.OK,login.StatusCode);
        c.DefaultRequestHeaders.Authorization=new("Bearer",(await login.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>());
        Assert.Equal(HttpStatusCode.OK,(await c.GetAsync(P+"reportes/opciones")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await c.GetAsync(P+"reportes/articulacion")).StatusCode);
    }
}
