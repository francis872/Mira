using Dapper;
using Microsoft.OpenApi;
using MIRA.Api.Configuracion;
using MIRA.Api.Excepciones;
using MIRA.Api.Repositorios;
using MIRA.Api.Servicios;

DefaultTypeMap.MatchNamesWithUnderscores = true;
var builder = WebApplication.CreateBuilder(args);
// Los scripts capturan consola; no depender de permisos del Event Log de Windows.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

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
        Version = "v2",
        Description = "MIRA - Catálogos V1 y base relacional V2 (sin autenticación en esta entrega)"
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
app.UseMiddleware<ApiExceptionMiddleware>();
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "MIRA API v2"));
}
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status="ok", timestamp=DateTime.UtcNow }));
app.Run();

public partial class Program { }
