using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Repositorios.Seguridad;
using MIRA.Api.Servicios.Seguridad;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class FunctionalRecoveryTests
{
    [RequiresDatabaseFact]
    public async Task CoordinatorCannotRetireChildrenThroughSheet_ReaderCanTraverseDetails_AdminCanRetire()
    {
        using var factory=TestHostFactory.Create(authenticated:false);
        using var scope=factory.Services.CreateScope();
        var repository=scope.ServiceProvider.GetRequiredService<ICuentaRepository>();
        var auth=scope.ServiceProvider.GetRequiredService<AuthService>();
        var passwords=scope.ServiceProvider.GetRequiredService<PasswordService>();
        var roles=await repository.Roles();
        async Task<HttpClient> Client(string role)
        {
            var roleId=roles.AsArray().Single(x=>x!["nombre"]!.GetValue<string>()==role)!["id"]!.GetValue<int>();
            var correo=Guid.NewGuid()+"@invalid.test";
            const string password="Isolated-recovery-validation-2026!";
            await repository.Crear(correo,passwords.Hash(password),[roleId]);
            var login=await auth.Login(new(correo,password));
            var client=factory.CreateClient();
            client.DefaultRequestHeaders.Authorization=new("Bearer",login!.Token);
            return client;
        }
        using var admin=await Client("Administrador");
        using var coordinator=await Client("Coordinador");
        using var reader=await Client("Investigador");
        var sedeResponse=await coordinator.PostAsJsonAsync("/api/investigacion/sedes",new{nombre="RECUPERACION seguridad",ciudad="Aislada",codigo_institucional="RECUPERACION"});
        Assert.Equal(HttpStatusCode.Created,sedeResponse.StatusCode);
        var sede=(await sedeResponse.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<int>();
        var group=new{sede_id=sede,gruptac_url="https://example.invalid/recovery",categoria="RECUPERACION"};
        var response=await coordinator.PostAsJsonAsync("/api/investigacion/grupos/ficha",new{grupo=group,semilleros=new[]{new{nombre="RECUPERACION hijo",descripcion="Aislada"}}});
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        var original=(await response.Content.ReadFromJsonAsync<JsonObject>())!;
        var id=original["grupo"]!["id"]!.GetValue<int>();
        var child=original["semilleros"]![0]!["id"]!.GetValue<int>();
        var path="/api/investigacion/grupos/ficha/"+id;
        var removal=new{grupo=new{sede_id=sede,gruptac_url="https://example.invalid/forbidden"},semilleros=Array.Empty<object>()};
        Assert.Equal(HttpStatusCode.Forbidden,(await coordinator.PutAsJsonAsync(path,removal)).StatusCode);
        Assert.True(JsonNode.DeepEquals(original,await admin.GetFromJsonAsync<JsonObject>(path)),"A denied omission must not change either master or children");
        var edit=new{grupo=group,semilleros=new[]{new{id=child,nombre="RECUPERACION hijo editado",descripcion="Aislada"}}};
        Assert.Equal(HttpStatusCode.OK,(await coordinator.PutAsJsonAsync(path,edit)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await coordinator.DeleteAsync("/api/investigacion/semilleros/"+child)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await reader.PutAsJsonAsync(path,edit)).StatusCode);
        var detail=(await reader.GetFromJsonAsync<JsonObject>($"/api/investigacion/grupos_investigacion/{id}/detalle"))!;
        Assert.Equal(child,detail["relaciones"]!["semilleros"]![0]!["id"]!.GetValue<int>());
        Assert.Equal("RECUPERACION hijo editado",detail["relaciones"]!["semilleros"]![0]!["nombre"]!.GetValue<string>());
        Assert.Contains(detail["referencias"]!["sedes"]!.AsArray(),n=>n!["nombre"]!.GetValue<string>()=="RECUPERACION seguridad");
        var refs=(await reader.GetFromJsonAsync<JsonObject>("/api/investigacion/referencias/catalogos"))!;
        foreach(var (_,items) in refs)
            foreach(var item in items!.AsArray())
                Assert.All(item!.AsObject(),field=>Assert.Contains(field.Key,new[]{"id","nombre","activo"}));
        Assert.Equal(HttpStatusCode.NotFound,(await reader.GetAsync("/api/investigacion/usuarios/1/detalle")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await reader.GetAsync("/api/investigacion/sedes/2147483647/detalle")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync(path,removal)).StatusCode);
        Assert.Empty((await admin.GetFromJsonAsync<JsonObject>(path))!["semilleros"]!.AsArray());
        // Historical details remain descriptive, never silently reassigned to a new parent.
        var historical=(await reader.GetFromJsonAsync<JsonObject>($"/api/investigacion/semilleros/{child}/detalle"))!;
        Assert.False(historical["registro"]!["activo"]!.GetValue<bool>());
        Assert.Equal(id,historical["registro"]!["grupo_id"]!.GetValue<int>());
        async Task<int> Reference(string resource)
        {
            var created=await coordinator.PostAsJsonAsync("/api/investigacion/"+resource,new{nombre="RECUPERACION "+Guid.NewGuid()});
            Assert.Equal(HttpStatusCode.Created,created.StatusCode);
            return (await created.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<int>();
        }
        var disciplina=await Reference("areas_conocimiento");
        var otra=await Reference("areas_conocimiento");
        var ods=await Reference("ods");
        var lineData=new{codigo=Guid.NewGuid().ToString("N"),nombre="RECUPERACION línea",descripcion="Aislada",disciplina_ids=new[]{disciplina,otra},ods_ids=new[]{ods}};
        var createdLine=await coordinator.PostAsJsonAsync("/api/investigacion/lineas_investigacion",lineData);
        Assert.Equal(HttpStatusCode.Created,createdLine.StatusCode);
        var line=(await createdLine.Content.ReadFromJsonAsync<JsonObject>())!;
        var linePath="/api/investigacion/lineas_investigacion/"+line["id"]!.GetValue<int>();
        var omitted=line.DeepClone().AsObject();
        // The API accepts only writable fields, not identifiers/defaults returned by PostgreSQL.
        foreach(var key in omitted.Select(x=>x.Key).Except(new[]{"codigo","nombre","descripcion","disciplina_ids","ods_ids"}).ToArray()) omitted.Remove(key);
        omitted["disciplina_ids"]=new JsonArray(disciplina);
        omitted["nombre"]="RECUPERACION retiro indebido";
        Assert.Equal(HttpStatusCode.Forbidden,(await coordinator.PutAsJsonAsync(linePath,omitted)).StatusCode);
        Assert.True(JsonNode.DeepEquals(line,await reader.GetFromJsonAsync<JsonObject>(linePath)),"Omitting a bridge must not save partial line changes");
        Assert.Equal(HttpStatusCode.OK,(await coordinator.PutAsJsonAsync(linePath,lineData)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync(linePath,omitted)).StatusCode);
        Assert.Single((await reader.GetFromJsonAsync<JsonObject>(linePath))!["disciplina_ids"]!.AsArray());
    }
}
