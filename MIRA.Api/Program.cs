using Dapper;
using Microsoft.OpenApi;
using MIRA.Api.Configuracion;
using MIRA.Api.Excepciones;
using MIRA.Api.Repositorios;
using MIRA.Api.Servicios;

DefaultTypeMap.MatchNamesWithUnderscores = true;
var builder = WebApplication.CreateBuilder(args);
var assignedPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(assignedPort) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    if (!int.TryParse(assignedPort, out var port) || port is < 1 or > 65535)
        throw new InvalidOperationException("PORT debe estar entre 1 y 65535.");
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
// Los scripts capturan consola; no depender de permisos del Event Log de Windows.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddSeguridad(builder.Configuration);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(entry => entry.Key, entry => entry.Value!.Errors.Select(error =>
                error.ErrorMessage.StartsWith("El campo ", StringComparison.Ordinal)
                    ? error.ErrorMessage : "El valor ingresado no es válido.").ToArray());
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
        {
            mensaje = "Revise los datos ingresados. Hay campos obligatorios o valores inválidos.",
            errors
        });
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "MIRA API",
        Version = "v3",
        Description = "MIRA V3 — autenticación JWT y permisos sobre catálogos V1 y dominio V2"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type=SecuritySchemeType.Http, Scheme="bearer", BearerFormat="JWT",
        Description="Obtenga el token mediante POST /api/auth/login e introdúzcalo aquí."
    });
    c.OperationFilter<SwaggerAnonymousOperationFilter>();
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IAreaConocimientoRepository, AreaConocimientoRepository>();
builder.Services.AddScoped<IObjetivoDesarrolloSostenibleRepository, ObjetivoDesarrolloSostenibleRepository>();
builder.Services.AddScoped<IAreaAplicacionRepository, AreaAplicacionRepository>();
builder.Services.AddScoped<ITerminoClaveRepository, TerminoClaveRepository>();
builder.Services.AddScoped<IUniversidadRepository, UniversidadRepository>();
builder.Services.AddScoped<ILineaInvestigacionRepository, LineaInvestigacionRepository>();
builder.Services.AddScoped<IAreaConocimientoService, AreaConocimientoService>();
builder.Services.AddScoped<IObjetivoDesarrolloSostenibleService, ObjetivoDesarrolloSostenibleService>();
builder.Services.AddScoped<IAreaAplicacionService, AreaAplicacionService>();
builder.Services.AddScoped<ITerminoClaveService, TerminoClaveService>();
builder.Services.AddScoped<IUniversidadService, UniversidadService>();
builder.Services.AddScoped<ILineaInvestigacionService, LineaInvestigacionService>();
builder.Services.AddScoped<MIRA.Api.Repositorios.Investigacion.InvestigacionRepository>();
builder.Services.AddScoped<InvestigacionService>();

var app = builder.Build();
// Validación de configuración local, sin abrir conexión ni publicar credenciales.
if (app.Environment.IsProduction()) _ = app.Services.GetRequiredService<IDbConnectionFactory>();
if (args.Contains("--bootstrap-admin") || args.Contains("--reset-admin-password"))
{
    using var scope = app.Services.CreateScope();
    var auth = scope.ServiceProvider.GetRequiredService<MIRA.Api.Servicios.Seguridad.AuthService>();
    var email = Environment.GetEnvironmentVariable("MIRA_ADMIN_EMAIL") ?? "";
    var password = Environment.GetEnvironmentVariable("MIRA_ADMIN_PASSWORD") ?? "";
    if (args.Contains("--reset-admin-password"))
        await auth.RestablecerAdmin(email, password);
    else
        await auth.Bootstrap(email, password);
    Console.WriteLine("Operacion administrativa completada.");
    return;
}
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Cache-Control"] = "no-store";
    await next(context);
});
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "MIRA API v3"));
}
app.MapControllers().RequireAuthorization("Datos");
app.MapGet("/health", () => Results.Ok(new { status="ok", timestamp=DateTime.UtcNow }));
app.Run();

public partial class Program { }
