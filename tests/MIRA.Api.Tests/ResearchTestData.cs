using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace MIRA.Api.Tests;

internal static class ResearchTestData
{
    // Valid fixtures for other contracts. Negative acceptance tests send their raw incomplete body directly.
    public static async Task<JsonObject> WithLineLinks(HttpClient client,string resource,object payload)
    {
        var data=payload as JsonObject ?? JsonSerializer.SerializeToNode(payload)!.AsObject();
        if(resource!="lineas_investigacion") return data;
        foreach(var (field,reference) in new[]{("disciplina_ids","areas_conocimiento"),("ods_ids","ods")})
        {
            if(data.ContainsKey(field)) continue;
            var response=await client.PostAsJsonAsync("/api/investigacion/"+reference,new {nombre="Referencia F5 "+Guid.NewGuid()});
            Assert.Equal(HttpStatusCode.Created,response.StatusCode);
            var id=(await response.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<int>();
            data[field]=new JsonArray(id);
        }
        return data;
    }
}
