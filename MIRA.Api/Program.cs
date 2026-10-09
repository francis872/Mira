using Dapper;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MIRA.Api.Configuracion;
using MIRA.Api.Repositorios;
using MIRA.Api.Servicios;
using MIRA.Api.Seguridad;

DefaultTypeMap.MatchNamesWithUnderscores = true;
var bootstrapAdmin = args.Length == 1 && args[0] == "--bootstrap-admin";
var builder = WebApplication.CreateBuilder(bootstrapAdmin ? [] : args);
if (bootstrapAdmin)
{
    await AdminBootstrapper.RunAsync(builder.Configuration);
    return;
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "MIRA API",
        Version = "v2",
        Description = "MIRA - Catálogos V1 y módulo de autenticación/usuarios-roles V2"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer",
        BearerFormat = "JWT", In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = new List<string>()
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
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

var key = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
    throw new InvalidOperationException("Configure Jwt:Secret mediante variable de entorno, con al menos 32 bytes.");
var issuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Configure Jwt:Issuer.");
var audience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Configure Jwt:Audience.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true, ValidIssuer = issuer,
            ValidateAudience = true, ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "MIRA API v2"));
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status="ok", timestamp=DateTime.UtcNow }));
app.Run();

public partial class Program { }
