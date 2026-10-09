using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MIRA.Api.Seguridad;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace MIRA.Api.Tests;

internal static class TestHostFactory
{
    public const string JwtSecret = "integration-test-signing-key-never-deploy-this";

    static TestHostFactory()
    {
        // Program.cs lee estos valores durante el arranque, antes de ConfigureAppConfiguration.
        Environment.SetEnvironmentVariable("Jwt__Secret", JwtSecret);
        Environment.SetEnvironmentVariable("ConnectionStrings__PostgreSql",
            Environment.GetEnvironmentVariable("MIRA_TEST_DB") ?? "Host=localhost;Database=unused;Username=unused;Password=unused");
    }

    public static WebApplicationFactory<Program> Create(Action<IServiceCollection>? configure = null)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            if (configure is not null)
            {
                builder.ConfigureTestServices(configure);
            }
        });

    public static string Token(string role, int expiredMinutesAgo = 0)
    {
        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var token = new JwtSecurityToken(
            ParametrosSeguridad.Emisor,
            ParametrosSeguridad.Audiencia,
            [new Claim(ClaimTypes.NameIdentifier, "test-user"), new Claim(ClaimTypes.Role, role)],
            notBefore: expiredMinutesAgo > 0 ? now.AddMinutes(-expiredMinutesAgo - 10) : now.AddMinutes(-1),
            expires: expiredMinutesAgo > 0 ? now.AddMinutes(-expiredMinutesAgo) : now.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
