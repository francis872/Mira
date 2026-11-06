using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using MIRA.Api.Repositorios.Seguridad;
using MIRA.Api.Servicios.Seguridad;

namespace MIRA.Api.Configuracion;

public static class SeguridadConfiguration
{
    public static IServiceCollection AddSeguridad(this IServiceCollection services, IConfiguration config)
    {
        var key = config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || System.Text.Encoding.UTF8.GetByteCount(key) < 32 || key.StartsWith("REPLACE"))
            throw new InvalidOperationException("Configure Jwt__Key con una clave aleatoria de al menos 32 bytes; no use valores de ejemplo.");
        var minutes = config.GetValue("Jwt:Minutes", 20);
        if (minutes is < 1 or > 60) throw new InvalidOperationException("Jwt__Minutes debe estar entre 1 y 60.");
        var settings = new JwtSettings { Key=key, Issuer=config["Jwt:Issuer"] ?? "MIRA", Audience=config["Jwt:Audience"] ?? "MIRA.Frontend", Minutes=minutes };
        if (string.IsNullOrWhiteSpace(settings.Issuer) || string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("Jwt__Issuer y Jwt__Audience no pueden estar vacíos.");
        services.AddSingleton(settings);
        services.AddSingleton<PasswordService>();
        services.AddScoped<ICuentaRepository, CuentaRepository>();
        services.AddScoped<AuthService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = settings.Validation();
            options.IncludeErrorDetails = false;
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var service = context.HttpContext.RequestServices.GetRequiredService<AuthService>();
                    if (!await service.ValidarSesion(context.Principal!)) context.Fail("Sesión inválida.");
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.Headers.WWWAuthenticate = "Bearer";
                    await context.Response.WriteAsJsonAsync(new { mensaje="Inicie sesión nuevamente para continuar." });
                },
                OnForbidden = async context => await context.Response.WriteAsJsonAsync(new { mensaje="Su cuenta no tiene permiso para realizar esta operación." })
            };
        });
        services.AddAuthorizationBuilder().AddPolicy("Datos", policy => policy.RequireAuthenticatedUser().RequireAssertion(context =>
        {
            var http = context.Resource as HttpContext;
            return http?.Request.Method switch
            {
                "GET" or "HEAD" => AuthService.RolesPermitidos.Any(context.User.IsInRole),
                "DELETE" => context.User.IsInRole("Administrador"),
                _ => context.User.IsInRole("Administrador") || context.User.IsInRole("Coordinador") ||
                    // El usuario puede cerrar su propia sesión, cualquiera que sea su rol conocido.
                    (http?.Request.Path == "/api/auth/logout" && AuthService.RolesPermitidos.Any(context.User.IsInRole))
            };
        }));
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddPolicy("login", http => RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                { PermitLimit=10, Window=TimeSpan.FromMinutes(1), QueueLimit=0 }));
            options.OnRejected = async (context, ct) => await context.HttpContext.Response.WriteAsJsonAsync(
                new { mensaje="Demasiados intentos. Espere un minuto antes de volver a intentarlo." }, ct);
        });
        return services;
    }
}
