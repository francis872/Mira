using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MIRA.Api.Configuracion;
using MIRA.Api.Repositorios;
using MIRA.Api.Servicios;

// Cargar variables desde archivo .env local (excluido en .gitignore) cuando se ejecuta fuera de Docker
foreach (var envPath in new[] { ".env", "../.env" })
{
    if (File.Exists(envPath))
    {
        foreach (var line in File.ReadAllLines(envPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
            var idx = trimmed.IndexOf('=');
            if (idx > 0)
            {
                var k = trimmed[..idx].Trim();
                var v = trimmed[(idx + 1)..].Trim();
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(k)))
                    Environment.SetEnvironmentVariable(k, v);
            }
        }
        break;
    }
}

// Habilitar mapeo automático de columnas snake_case de PostgreSQL a propiedades PascalCase en Dapper
DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// 1. Controladores
builder.Services.AddControllers();

// 2. Swagger con soporte para Tokens JWT (Botón Authorize y Candado)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MIRA API",
        Version = "v0.4",
        Description = "Módulo de Investigación - API Completa (Catálogos, Relaciones, Seguridad JWT/RBAC y 10 Consultas Multitabla)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación JWT usando el esquema Bearer. \r\n\r\n Ingrese 'Bearer' [espacio] y luego su token en el campo.\r\n\r\nEjemplo: \"Bearer eyJhbGci...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// 3. Configuración de Autenticación JWT leyendo de variables de entorno sin secretos quemados
var configuredJwtKey = builder.Configuration["JwtSettings:Key"];
var jwtKey = (!string.IsNullOrWhiteSpace(configuredJwtKey) && !configuredJwtKey.Contains("<TU_"))
    ? configuredJwtKey
    : (Environment.GetEnvironmentVariable("JWT_SECRET") ?? "MIRA_Dev_Fallback_Key_2026_Min32Bytes_Length_OK!");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "MIRA_Api";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "MIRA_Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// 4. Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// =============================================================================
// INYECCIÓN DE DEPENDENCIAS (Conexión, Repositorios y Servicios con Dapper)
// =============================================================================

// Fábrica de conexión a PostgreSQL
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// --- FASE 1: Catálogos Base ---
builder.Services.AddScoped<IAreaConocimientoRepository, AreaConocimientoRepository>();
builder.Services.AddScoped<IAreaConocimientoService, AreaConocimientoService>();

builder.Services.AddScoped<IObjetivoDesarrolloSostenibleRepository, ObjetivoDesarrolloSostenibleRepository>();
builder.Services.AddScoped<IObjetivoDesarrolloSostenibleService, ObjetivoDesarrolloSostenibleService>();

builder.Services.AddScoped<IAreaAplicacionRepository, AreaAplicacionRepository>();
builder.Services.AddScoped<IAreaAplicacionService, AreaAplicacionService>();

builder.Services.AddScoped<ITerminoClaveRepository, TerminoClaveRepository>();
builder.Services.AddScoped<ITerminoClaveService, TerminoClaveService>();

builder.Services.AddScoped<IUniversidadRepository, UniversidadRepository>();
builder.Services.AddScoped<IUniversidadService, UniversidadService>();

builder.Services.AddScoped<ILineaInvestigacionRepository, LineaInvestigacionRepository>();
builder.Services.AddScoped<ILineaInvestigacionService, LineaInvestigacionService>();

// --- FASE 2: Entidades Relacionales y Articulación ---
builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
builder.Services.AddScoped<IDocenteService, DocenteService>();

builder.Services.AddScoped<IGrupoInvestigacionRepository, GrupoInvestigacionRepository>();
builder.Services.AddScoped<IGrupoInvestigacionService, GrupoInvestigacionService>();

builder.Services.AddScoped<ISemilleroRepository, SemilleroRepository>();
builder.Services.AddScoped<ISemilleroService, SemilleroService>();

builder.Services.AddScoped<IRelacionesRepository, RelacionesRepository>();
builder.Services.AddScoped<IRelacionesService, RelacionesService>();

// --- FASE 3: Seguridad, Usuarios y Roles ---
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// --- FASE 4: Reportes Multitabla y Dashboard Ejecutivo (v0.4) ---
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
builder.Services.AddScoped<IReporteService, ReporteService>();

// =============================================================================
// PIPELINE HTTP
// =============================================================================
var app = builder.Build();

// Mostrar siempre detalles de excepción durante la fase de despliegue
app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MIRA API v0.4");
});

app.UseCors("CorsPolicy");

// CRÍTICO: UseAuthentication SIEMPRE debe ir antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
