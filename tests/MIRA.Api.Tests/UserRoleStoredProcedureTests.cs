using Dapper;
using Microsoft.Extensions.Configuration;
using MIRA.Api.Configuracion;
using MIRA.Api.Seguridad;
using Npgsql;
using Xunit;

namespace MIRA.Api.Tests;

/// <summary>Se omite si MIRA_TEST_DB no apunta a una base PostgreSQL inicializada con database/init.</summary>
public sealed class RequiresDatabaseFactAttribute : FactAttribute
{
    public RequiresDatabaseFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MIRA_TEST_DB")))
        {
            Skip = "Defina MIRA_TEST_DB con la cadena de conexión de una base de pruebas.";
        }
    }
}

public sealed class UserRoleStoredProcedureTests : IDisposable
{
    private const string Prefix = "sp-test-";
    private readonly IDbConnectionFactory _factory;
    private readonly AuthRepository _repository;

    public UserRoleStoredProcedureTests()
    {
        var connection = Environment.GetEnvironmentVariable("MIRA_TEST_DB") ?? "Host=unused";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:PostgreSql"] = connection })
            .Build();
        _factory = new DbConnectionFactory(configuration);
        _repository = new AuthRepository(_factory);
    }

    private static string Hash() => new('x', 60);

    private async Task<int[]> RoleIdsAsync(params string[] names)
    {
        var roles = await _repository.RolesActivosAsync();
        return names.Select(name => roles.Single(role => role.Nombre == name).Id).ToArray();
    }

    private async Task<int> CountUsersAsync(string email)
    {
        using var db = _factory.CreateConnection();
        return await db.ExecuteScalarAsync<int>("SELECT count(*) FROM usuario WHERE correo=@email", new { email });
    }

    [RequiresDatabaseFact]
    public async Task Create_InsertsMasterAndAllDetailsAtomically()
    {
        var roles = await RoleIdsAsync("Investigador", "Coordinador");

        var json = await _repository.CrearAsync($"{Prefix}ok@example.invalid", Hash(), roles);

        Assert.Contains("Investigador", json);
        Assert.Contains("Coordinador", json);
    }

    [RequiresDatabaseFact]
    public async Task Create_WithNonexistentRole_RollsBackTheMaster()
    {
        var email = $"{Prefix}rollback@example.invalid";

        var error = await Assert.ThrowsAsync<PostgresException>(() => _repository.CrearAsync(email, Hash(), [int.MaxValue]));

        Assert.Equal("23503", error.SqlState);
        Assert.Equal(0, await CountUsersAsync(email));
    }

    [RequiresDatabaseFact]
    public async Task Create_WithDuplicateEmail_RaisesUniqueViolation()
    {
        var roles = await RoleIdsAsync("Investigador");
        var email = $"{Prefix}dup@example.invalid";
        await _repository.CrearAsync(email, Hash(), roles);

        var error = await Assert.ThrowsAsync<PostgresException>(() => _repository.CrearAsync(email, Hash(), roles));

        Assert.Equal("23505", error.SqlState);
    }

    [RequiresDatabaseFact]
    public async Task Update_ReplacesDetailsAndMissingUserRaisesP0002()
    {
        var created = await _repository.CrearAsync($"{Prefix}upd@example.invalid", Hash(), await RoleIdsAsync("Investigador"));
        var id = System.Text.Json.JsonDocument.Parse(created).RootElement.GetProperty("id").GetInt64();

        var updated = await _repository.ActualizarAsync(id, $"{Prefix}upd2@example.invalid", await RoleIdsAsync("Coordinador"));

        Assert.Contains("Coordinador", updated);
        Assert.DoesNotContain("Investigador", updated);
        var error = await Assert.ThrowsAsync<PostgresException>(() => _repository.ActualizarAsync(long.MaxValue, "x@example.invalid", [1]));
        Assert.Equal("P0002", error.SqlState);
    }

    [RequiresDatabaseFact]
    public async Task Update_WithInvalidRole_KeepsPreviousDetails()
    {
        var email = $"{Prefix}keep@example.invalid";
        var created = await _repository.CrearAsync(email, Hash(), await RoleIdsAsync("Investigador"));
        var id = System.Text.Json.JsonDocument.Parse(created).RootElement.GetProperty("id").GetInt64();

        await Assert.ThrowsAsync<PostgresException>(() => _repository.ActualizarAsync(id, $"{Prefix}changed@example.invalid", [int.MaxValue]));

        var current = await _repository.ConsultarAsync(id);
        Assert.Contains(email, current);
        Assert.Contains("Investigador", current);
    }

    [RequiresDatabaseFact]
    public async Task Deactivate_IsLogicalAndIdempotentlyReportsMissing()
    {
        var created = await _repository.CrearAsync($"{Prefix}off@example.invalid", Hash(), await RoleIdsAsync("Investigador"));
        var id = System.Text.Json.JsonDocument.Parse(created).RootElement.GetProperty("id").GetInt64();

        Assert.True(await _repository.InactivarAsync(id));
        Assert.False(await _repository.InactivarAsync(id));
        Assert.NotNull(await _repository.ConsultarAsync(id));
    }

    [RequiresDatabaseFact]
    public async Task BootstrapAdmin_CannotProvisionTwoFirstAdministrators()
    {
        try
        {
            await _repository.BootstrapAdminAsync($"{Prefix}boot1@example.invalid", Hash());
        }
        catch (PostgresException first) when (first.SqlState == "23505")
        {
            // Ya existía un administrador activo: la regla se cumple desde el primer intento.
        }

        var error = await Assert.ThrowsAsync<PostgresException>(() => _repository.BootstrapAdminAsync($"{Prefix}boot2@example.invalid", Hash()));
        Assert.Equal("23505", error.SqlState);
    }

    public void Dispose()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MIRA_TEST_DB")))
        {
            return;
        }

        using var db = _factory.CreateConnection();
        db.Execute($"DELETE FROM usuario_rol WHERE usuario_id IN (SELECT id FROM usuario WHERE correo LIKE '{Prefix}%')");
        db.Execute($"DELETE FROM usuario WHERE correo LIKE '{Prefix}%'");
    }
}
