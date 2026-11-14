using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;

namespace MIRA.Api.Configuracion;

/// <summary>A separate server-to-server key authenticates Flask's origin assertion, never a user header.</summary>
public sealed class LoginOriginSettings
{
    public const string Header = "X-Mira-Login-Origin";
    public const int MaxBodyBytes = 8192;
    public byte[]? Key { get; }

    public LoginOriginSettings(IConfiguration configuration, IHostEnvironment environment)
    {
        var key = configuration["LoginProtection:ForwardingKey"] ?? "";
        if ((environment.IsProduction() || key.Length > 0) &&
            (Encoding.UTF8.GetByteCount(key) < 32 || key.StartsWith("REPLACE", StringComparison.Ordinal)))
            throw new InvalidOperationException("Configure LoginProtection__ForwardingKey con una clave externa independiente de al menos 32 bytes, compartida solo con Flask.");
        if (key.Length > 0 && key == configuration["Jwt:Key"])
            throw new InvalidOperationException("LoginProtection__ForwardingKey debe ser diferente de Jwt__Key.");
        Key = key.Length == 0 ? null : Encoding.UTF8.GetBytes(key);
    }

    public static bool IsLogin(HttpContext context) =>
        context.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName == "login";

    public static string CanonicalIp(IPAddress? address) => address is null ? "unknown" :
        (address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address).ToString();

    public static string Payload(string timestamp, string ip, string method, string path, ReadOnlySpan<byte> body) =>
        $"mira-login-v1\n{timestamp}\n{ip}\n{method.ToUpperInvariant()}\n{path.TrimEnd('/').ToLowerInvariant()}\n{Convert.ToHexStringLower(SHA256.HashData(body))}";

    public bool TryReadOrigin(HttpContext context, ReadOnlySpan<byte> body, out string origin)
    {
        origin = CanonicalIp(context.Connection.RemoteIpAddress);
        if (!context.Request.Headers.TryGetValue(Header, out var values)) return true;
        if (Key is null || values.Count != 1 || values[0] is not { Length: <= 200 } header) return false;
        var parts = header.Split(';');
        if (parts.Length != 4 || parts[0] != "v1" || parts[1].Length is < 1 or > 12 ||
            !parts[1].All(char.IsAsciiDigit) ||
            !long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp) ||
            Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp) > 60 ||
            parts[2].Contains('%') || !IPAddress.TryParse(parts[2], out var ip) ||
            CanonicalIp(ip) != parts[2] || parts[3].Length != 64) return false;
        byte[] signature;
        try { signature = Convert.FromHexString(parts[3]); }
        catch (FormatException) { return false; }
        var expected = HMACSHA256.HashData(Key, Encoding.UTF8.GetBytes(
            Payload(parts[1], parts[2], context.Request.Method, context.Request.Path, body)));
        if (!CryptographicOperations.FixedTimeEquals(expected, signature)) return false;
        origin = parts[2];
        return true;
    }
}

/// <summary>Bounded login parsing before rate limiting; leaves the body available to MVC.</summary>
public sealed class LoginProtectionMiddleware(RequestDelegate next)
{
    public const string OriginItem = "mira.login.origin";
    public const string AccountItem = "mira.login.account";

    public async Task InvokeAsync(HttpContext context, LoginOriginSettings settings)
    {
        if (!LoginOriginSettings.IsLogin(context)) { await next(context); return; }
        if (context.Request.ContentLength > LoginOriginSettings.MaxBodyBytes)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsJsonAsync(new { mensaje = "La solicitud de acceso es demasiado grande." });
            return;
        }
        context.Request.EnableBuffering(LoginOriginSettings.MaxBodyBytes, LoginOriginSettings.MaxBodyBytes + 1);
        var body = new byte[LoginOriginSettings.MaxBodyBytes + 1];
        var length = 0;
        while (length < body.Length)
        {
            var read = await context.Request.Body.ReadAsync(body.AsMemory(length), context.RequestAborted);
            if (read == 0) break;
            length += read;
        }
        context.Request.Body.Position = 0;
        if (length > LoginOriginSettings.MaxBodyBytes)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsJsonAsync(new { mensaje = "La solicitud de acceso es demasiado grande." });
            return;
        }
        if (!settings.TryReadOrigin(context, body.AsSpan(0, length), out var origin))
        {
            // No credentials are checked if server-to-server origin authentication fails.
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { mensaje = "Correo o contraseña incorrectos, o cuenta no disponible." });
            return;
        }
        context.Items[OriginItem] = origin;
        var account = "invalid";
        try
        {
            using var json = JsonDocument.Parse(body.AsMemory(0, length));
            if (json.RootElement.ValueKind == JsonValueKind.Object)
                foreach (var property in json.RootElement.EnumerateObject())
                    if (property.Name.Equals("correo", StringComparison.OrdinalIgnoreCase))
                        account = property.Value.ValueKind == JsonValueKind.String
                            ? property.Value.GetString()!.Trim().ToLowerInvariant() : "invalid";
        }
        catch (JsonException) { /* MVC produces the existing validation response. */ }
        // Do not retain email addresses in partition keys or expose them in responses/logs.
        context.Items[AccountItem] = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(account)));
        await next(context);
    }
}
