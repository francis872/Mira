using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Security.Claims;

namespace MIRA.Api.Tests;

internal static class TestHostFactory
{
    static TestHostFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", "isolated-tests-only-32-byte-signing-key-do-not-deploy");
        Environment.SetEnvironmentVariable("ConnectionStrings__PostgreSql",
            Environment.GetEnvironmentVariable("MIRA_TEST_DB") ?? "Host=localhost;Database=unused;Username=unused;Password=unused");
    }

    public static WebApplicationFactory<Program> Create(Action<IServiceCollection>? configure = null, bool authenticated = true)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string,string?>
            {
                ["Jwt:Key"]="isolated-tests-only-32-byte-signing-key-do-not-deploy"
            }));
            builder.ConfigureTestServices(services =>
            {
                if (authenticated)
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme="Regression";
                        options.DefaultChallengeScheme="Regression";
                        options.DefaultForbidScheme="Regression";
                    }).AddScheme<AuthenticationSchemeOptions, RegressionAuthentication>("Regression", _ => { });
                configure?.Invoke(services);
            });
        });
}

// Solo para regresiones V1/V2: conserva sus escenarios de negocio con identidad explícita.
internal sealed class RegressionAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity([new Claim("sub", "1"), new Claim("email", "regression@invalid.test"), new Claim("role", "Administrador")], Scheme.Name, "email", "role");
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
