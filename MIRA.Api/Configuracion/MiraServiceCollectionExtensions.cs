using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MIRA.Api.Repositorios;
using MIRA.Api.Servicios;

namespace MIRA.Api.Configuracion;

public static class MiraServiceCollectionExtensions
{
    public const string CorsPolicyName = "MiraLocalFrontend";

    public static IServiceCollection AddMiraConfiguracion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        var jwt = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddCors(options =>
        {
            options.AddPolicy(
                CorsPolicyName,
                policy =>
                    policy
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
        });

        services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<ISedeRepositorio, SedeRepositorio>();
        services.AddScoped<ICatalogoRepositorio, CatalogoRepositorio>();

        services.AddScoped<IAuthServicio, AuthServicio>();
        services.AddScoped<ISedeServicio, SedeServicio>();
        services.AddScoped<ICatalogoServicio, CatalogoServicio>();

        return services;
    }
}
