using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios.Seguridad;
using MIRA.Api.Servicios.Seguridad;
using Npgsql;
using Xunit;

namespace MIRA.Api.Tests;

public sealed class SeguridadDatabaseTests
{
    [RequiresDatabaseFact]
    public async Task RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration()
    {
        using var factory=TestHostFactory.Create(authenticated:false);
        using var scope=factory.Services.CreateScope();
        var repository=scope.ServiceProvider.GetRequiredService<ICuentaRepository>();
        var passwords=new PasswordService();
        var prefix=Guid.NewGuid().ToString("N");
        const string password="Isolated-password-2026!";
        await using var db=new NpgsqlConnection(Environment.GetEnvironmentVariable("MIRA_TEST_DB"));
        await db.OpenAsync();
        async Task<int> Role(string name)
        {
            await using var command=new NpgsqlCommand("SELECT id FROM rol WHERE nombre=@name",db);
            command.Parameters.AddWithValue("name",name);
            return (int)(await command.ExecuteScalarAsync())!;
        }
        var admin=await repository.Crear(prefix+"-admin@invalid.test",passwords.Hash(password),[await Role("Administrador")]);
        Assert.NotNull(await repository.Buscar(prefix+"-admin@invalid.test"));
        using var client=factory.CreateClient();
        async Task<string> Login(string email,string value=password)
        {
            var response=await client.PostAsJsonAsync("/api/auth/login",new {correo=email,password=value});
            Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
        }
        client.DefaultRequestHeaders.Authorization=new("Bearer",await Login(prefix+"-admin@invalid.test"));
        var email=prefix+"-reader@invalid.test";
        var response=await client.PostAsJsonAsync("/api/auth/usuarios",new {correo=email,password,roles=new[]{await Role("Investigador")}});
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        var body=(await response.Content.ReadFromJsonAsync<JsonObject>())!;
        var id=body["id"]!.GetValue<long>();
        Assert.DoesNotContain("password",body.ToJsonString(),StringComparison.OrdinalIgnoreCase);
        var stored=await repository.Buscar(email);
        Assert.True(passwords.Verify(password,stored!.PasswordHash));
        Assert.NotEqual(password,stored.PasswordHash);
        var readerToken=await Login(email);
        using var reader=factory.CreateClient(); reader.DefaultRequestHeaders.Authorization=new("Bearer",readerToken);
        Assert.Equal(HttpStatusCode.OK,(await reader.GetAsync("/api/universidad")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await reader.GetAsync("/api/auth/usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync($"/api/auth/usuarios/{id}",new {correo=email,roles=new[]{await Role("Coordinador")}})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await reader.GetAsync("/api/auth/me")).StatusCode);
        reader.DefaultRequestHeaders.Authorization=new("Bearer",await Login(email));
        const string replacement="Replacement-password-2026!";
        Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync($"/api/auth/usuarios/{id}/password",new {password=replacement})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await reader.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/auth/login",new {correo=email,password})).StatusCode);
        reader.DefaultRequestHeaders.Authorization=new("Bearer",await Login(email,replacement));
        Assert.Equal(HttpStatusCode.OK,(await reader.PostAsJsonAsync("/api/auth/logout",new {})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await reader.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.DeleteAsync($"/api/auth/usuarios/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/auth/login",new {correo=email,password=replacement})).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await client.PutAsJsonAsync("/api/auth/usuarios/9223372036854775807",new {correo=email,roles=new[]{await Role("Investigador")}})).StatusCode);
        // Recuperar un administrador heredado sin contraseña bcrypt exige la vía operativa local.
        await using (var legacy = new NpgsqlCommand("UPDATE usuario SET password_hash='NO_ES_BCRYPT' WHERE id=@id",db))
        {
            legacy.Parameters.AddWithValue("id",admin["id"]!.GetValue<long>());
            await legacy.ExecuteNonQueryAsync();
        }
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PostAsJsonAsync("/api/auth/login",new {correo=prefix+"-admin@invalid.test",password})).StatusCode);
        await scope.ServiceProvider.GetRequiredService<AuthService>().RestablecerAdmin(prefix+"-admin@invalid.test",replacement);
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync("/api/auth/me")).StatusCode);
        _ = await Login(prefix+"-admin@invalid.test",replacement);
        // La base es desechable; las cuentas de pruebas se conservan inactivas para trazabilidad.
        await repository.Inactivar(admin["id"]!.GetValue<long>());
    }
}
