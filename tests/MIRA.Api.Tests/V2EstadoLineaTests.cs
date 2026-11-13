using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Repositorios.Seguridad;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class V2EstadoLineaTests
{
    private const string P="/api/investigacion/";
    private static async Task<int> Line(HttpClient client)
    {
        var data=await ResearchTestData.WithLineLinks(client,"lineas_investigacion",new {codigo=Guid.NewGuid().ToString("N"),nombre="Estado F5",descripcion="Descripción"});
        var response=await client.PostAsJsonAsync(P+"lineas_investigacion",data);Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<int>();
    }
    [RequiresDatabaseTheory]
    [InlineData("Investigador",HttpStatusCode.Forbidden)]
    [InlineData("Coordinador",HttpStatusCode.Forbidden)]
    [InlineData("Administrador",HttpStatusCode.OK)]
    public async Task StateChangesKeepTheExistingAdministrativeRestriction(string role,HttpStatusCode expected)
    {
        using var admin=TestHostFactory.Create();using var setup=admin.CreateClient();var id=await Line(setup);
        Assert.Equal(HttpStatusCode.OK,(await setup.DeleteAsync(P+"lineas_investigacion/"+id)).StatusCode);
        var repo=new SeguridadTests.CuentaStub {RolesActuales=[role]};
        using var f=TestHostFactory.Create(s=>s.AddSingleton<ICuentaRepository>(repo),authenticated:false);using var client=f.CreateClient();
        var login=await client.PostAsJsonAsync("/api/auth/login",new {correo="test@invalid.test",password="Password-test-2026!"});
        client.DefaultRequestHeaders.Authorization=new("Bearer",(await login.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>());
        Assert.Equal(expected,(await client.GetAsync(P+"lineas_investigacion/inactivas")).StatusCode);
        var response=await client.PutAsJsonAsync(P+$"lineas_investigacion/{id}/estado",new {activo=true});
        Assert.Equal(expected,response.StatusCode);
        if(expected==HttpStatusCode.OK)
        {
            var row=(await response.Content.ReadFromJsonAsync<JsonObject>())!;
            Assert.True(row["activo"]!.GetValue<bool>());Assert.Single(row["ods_ids"]!.AsArray());Assert.Single(row["disciplina_ids"]!.AsArray());
            Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync(P+$"lineas_investigacion/{id}/estado",new {activo=false})).StatusCode);
        }
    }
    [RequiresDatabaseFact]
    public async Task MissingStateDoesNotImplicitlyInactivateALine()
    {
        using var f=TestHostFactory.Create();using var client=f.CreateClient();var id=await Line(client);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PutAsJsonAsync(P+$"lineas_investigacion/{id}/estado",new{})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync(P+"lineas_investigacion/"+id)).StatusCode);
    }
    [RequiresDatabaseFact]
    public async Task NewWriteRoutesRemainProtectedAgainstAnonymousRequests()
    {
        using var f=TestHostFactory.Create(authenticated:false);using var client=f.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PutAsJsonAsync(P+"lineas_investigacion/1/estado",new {activo=true})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync(P+"asociaciones/grupo_lineas/asignar",new {padreId=1,lineas=new[]{1}})).StatusCode);
    }
}
