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

        services.AddScoped<ISedeServicio, SedeServicio>();
        services.AddScoped<ICatalogoServicio, CatalogoServicio>();

        return services;
    }
}
