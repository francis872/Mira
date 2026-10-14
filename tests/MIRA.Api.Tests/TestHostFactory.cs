using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MIRA.Api.Tests;

internal static class TestHostFactory
{
    static TestHostFactory()
    {
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
}
