using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class V1RequisitosOriginalesTests
{
    [RequiresDatabaseTheory]
    [InlineData(null)] [InlineData("")] [InlineData("   ")] [InlineData("\t")]
    public async Task KeywordRequiresEnglishOnCreateAndUpdate(string? english)
    {
        using var factory=TestHostFactory.Create(); using var client=factory.CreateClient();
        var term="f5-english-"+Guid.NewGuid().ToString("N");
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/termino_clave",new {termino=term,terminoIngles=english})).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync("/api/termino_clave/"+term)).StatusCode);
        Assert.Equal(HttpStatusCode.Created,(await client.PostAsJsonAsync("/api/termino_clave",new {termino=term,terminoIngles="Verified equivalent"})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PutAsJsonAsync("/api/termino_clave/"+term,new {terminoIngles=english})).StatusCode);
        var saved=await client.GetFromJsonAsync<JsonElement>("/api/termino_clave/"+term);
        Assert.Equal("Verified equivalent",saved.GetProperty("terminoIngles").GetString());
        Assert.Equal(HttpStatusCode.OK,(await client.DeleteAsync("/api/termino_clave/"+term)).StatusCode);
    }

    [RequiresDatabaseTheory]
    [InlineData("Social")] [InlineData("Económica")] [InlineData("Ambiental")]
    public async Task OnlyOriginalOdsCategoriesPersist(string category)
    {
        using var factory=TestHostFactory.Create(); using var client=factory.CreateClient();
        var response=await client.PostAsJsonAsync("/api/objetivo_desarrollo_sostenible",new {nombre="f5-ods-"+Guid.NewGuid().ToString("N"),categoria=category});
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        var created=await response.Content.ReadFromJsonAsync<JsonElement>(); var id=created.GetProperty("id").GetInt32();
        var saved=await client.GetFromJsonAsync<JsonElement>("/api/objetivo_desarrollo_sostenible/"+id);
        Assert.Equal(category,saved.GetProperty("categoria").GetString());
        Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync("/api/objetivo_desarrollo_sostenible/"+id,new {nombre="f5-editado",categoria=category})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.DeleteAsync("/api/objetivo_desarrollo_sostenible/"+id)).StatusCode);
    }

    [RequiresDatabaseTheory]
    [InlineData(null)] [InlineData("")] [InlineData("Económico")] [InlineData("Economica")]
    [InlineData("Environmental")] [InlineData("Otra")]
    public async Task InvalidOdsCategoryIsRejectedOnCreateAndUpdate(string? category)
    {
        using var factory=TestHostFactory.Create(); using var client=factory.CreateClient();
        var name="f5-ods-"+Guid.NewGuid().ToString("N");
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/objetivo_desarrollo_sostenible",new {nombre=name,categoria=category})).StatusCode);
        var create=await client.PostAsJsonAsync("/api/objetivo_desarrollo_sostenible",new {nombre=name,categoria="Social"});
        Assert.Equal(HttpStatusCode.Created,create.StatusCode);
        var id=(await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PutAsJsonAsync("/api/objetivo_desarrollo_sostenible/"+id,new {nombre=name,categoria=category})).StatusCode);
        var saved=await client.GetFromJsonAsync<JsonElement>("/api/objetivo_desarrollo_sostenible/"+id);
        Assert.Equal("Social",saved.GetProperty("categoria").GetString());
        Assert.Equal(HttpStatusCode.OK,(await client.DeleteAsync("/api/objetivo_desarrollo_sostenible/"+id)).StatusCode);
    }
}
