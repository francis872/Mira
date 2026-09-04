using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MIRA.Api.IntegrationTests;

public class AuthEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Login_ThenMe_ReturnsCurrentUser()
    {
        var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            correo = "admin@mira.local",
            password = "Admin123!"
        });

        loginResponse.EnsureSuccessStatusCode();
        var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginJson.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var meResponse = await client.GetAsync("/api/auth/me");
        meResponse.EnsureSuccessStatusCode();

        var meJson = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("admin@mira.local", meJson.GetProperty("correo").GetString());
    }
}
