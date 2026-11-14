using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Npgsql;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class V2MinimosLineaTests
{
    private const string P="/api/investigacion/";
    [RequiresDatabaseFact]
    public async Task AllSeventeenGoalsAreAvailableAndCanBeSelectedTogether()
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var goals=(await c.GetFromJsonAsync<JsonArray>(P+"ods"))!;
        var ids=Enumerable.Range(1,17).Select(number=>goals.Single(g=>g!["nombre"]!.GetValue<string>().StartsWith($"ODS {number} - "))!["id"]!.GetValue<int>()).ToArray();
        var data=await Data(c);data["ods_ids"]=new JsonArray(ids.Select(i=>JsonValue.Create(i)).ToArray());
        var id=await Add(c,"lineas_investigacion",data);
        var saved=(await c.GetFromJsonAsync<JsonObject>(P+"lineas_investigacion/"+id))!;
        Assert.Equal(ids.Order(),saved["ods_ids"]!.AsArray().Select(v=>v!.GetValue<int>()).Order());
    }
    private static Task<JsonObject> Data(HttpClient client) => ResearchTestData.WithLineLinks(client,"lineas_investigacion",new {codigo=Guid.NewGuid().ToString("N"),nombre="Línea íntegra",descripcion="Descripción académica"});
    private static async Task<int> Add(HttpClient client,string resource,object body)
    {
        var r=await client.PostAsJsonAsync(P+resource,body);Assert.True(r.StatusCode==HttpStatusCode.Created,await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<int>();
    }
    [RequiresDatabaseTheory]
    [InlineData("disciplina_ids",false)][InlineData("disciplina_ids",true)]
    [InlineData("ods_ids",false)][InlineData("ods_ids",true)]
    public async Task MissingOrEmptyMinimumIsRejectedOnCreateAndUpdate(string field,bool empty)
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var valid=await Data(c);var bad=(JsonObject)valid.DeepClone();
        if(empty)bad[field]=new JsonArray();else bad.Remove(field);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsJsonAsync(P+"lineas_investigacion",bad)).StatusCode);
        var id=await Add(c,"lineas_investigacion",valid);bad["nombre"]="No debe guardarse";
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PutAsJsonAsync(P+"lineas_investigacion/"+id,bad)).StatusCode);
        var saved=(await c.GetFromJsonAsync<JsonObject>(P+"lineas_investigacion/"+id))!;
        Assert.Equal("Línea íntegra",saved["nombre"]!.GetValue<string>());
    }
    [RequiresDatabaseFact]
    public async Task LateInvalidReferenceRollsBackLineAndBothAssociationSets()
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var data=await Data(c);var id=await Add(c,"lineas_investigacion",data);
        var before=(await c.GetFromJsonAsync<JsonObject>(P+"lineas_investigacion/"+id))!;
        data["nombre"]="Cambio revertido";data["ods_ids"]=new JsonArray(int.MaxValue);
        Assert.Equal(HttpStatusCode.Conflict,(await c.PutAsJsonAsync(P+"lineas_investigacion/"+id,data)).StatusCode);
        Assert.True(JsonNode.DeepEquals(before,await c.GetFromJsonAsync<JsonObject>(P+"lineas_investigacion/"+id)));
    }
    [RequiresDatabaseTheory]
    [InlineData("linea_ods","ods_ids")][InlineData("linea_disciplinas","disciplina_ids")]
    public async Task LastMandatoryAssociationCannotBeRemoved(string bridge,string field)
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var data=await Data(c);var id=await Add(c,"lineas_investigacion",data);var reference=data[field]![0]!.GetValue<int>();
        Assert.Equal(HttpStatusCode.Conflict,(await c.DeleteAsync($"{P}{bridge}/{id}/{reference}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await c.GetAsync($"{P}{bridge}/{id}/{reference}")).StatusCode);
    }
    [RequiresDatabaseFact]
    public async Task ConcurrentRemovalsCannotLeaveLineWithoutDiscipline()
    {
        using var f=TestHostFactory.Create();using var c=f.CreateClient();
        var data=await Data(c);var id=await Add(c,"lineas_investigacion",data);var first=data["disciplina_ids"]![0]!.GetValue<int>();
        var second=await Add(c,"areas_conocimiento",new {nombre="Segunda disciplina "+Guid.NewGuid()});
        Assert.Equal(HttpStatusCode.Created,(await c.PostAsJsonAsync(P+"linea_disciplinas",new {linea_id=id,disciplina_id=second})).StatusCode);
        async Task<bool> Remove(int key)
        {
            await using var db=new NpgsqlConnection(Environment.GetEnvironmentVariable("MIRA_TEST_DB"));await db.OpenAsync();await using var tx=await db.BeginTransactionAsync();
            try
            {
                await using var command=new NpgsqlCommand("DELETE FROM linea_disciplinas WHERE linea_id=@linea AND disciplina_id=@disciplina",db,tx);
                command.Parameters.AddWithValue("linea",id);command.Parameters.AddWithValue("disciplina",key);
                await command.ExecuteNonQueryAsync();await tx.CommitAsync();return true;
            }
            catch(PostgresException e) when(e.SqlState=="23503") {return false;}
        }
        var results=await Task.WhenAll(Remove(first),Remove(second));
        Assert.Single(results.Where(ok=>ok));
        var saved=(await c.GetFromJsonAsync<JsonObject>(P+"lineas_investigacion/"+id))!;
        Assert.Single(saved["disciplina_ids"]!.AsArray());
    }
}
