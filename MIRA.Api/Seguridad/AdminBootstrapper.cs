using System.Net.Mail;
using MIRA.Api.Configuracion;
using Npgsql;

namespace MIRA.Api.Seguridad;

public static class AdminBootstrapper
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var email = configuration["MIRA_BOOTSTRAP_ADMIN_EMAIL"];
        var password = configuration["MIRA_BOOTSTRAP_ADMIN_PASSWORD"];
        var normalizedEmail = email?.Trim();
        if (!EmailValido(normalizedEmail) || string.IsNullOrWhiteSpace(password) || password.Length < 12)
        {
            throw new InvalidOperationException(
                "Defina MIRA_BOOTSTRAP_ADMIN_EMAIL y una contraseña de al menos 12 caracteres en el entorno del proceso.");
        }

        var repository = new AuthRepository(new DbConnectionFactory(configuration));
        var hash = BCrypt.Net.BCrypt.HashPassword(password, ParametrosSeguridad.CostoBcrypt);
        try
        {
            await repository.BootstrapAdminAsync(normalizedEmail!, hash);
            Console.WriteLine("Primer administrador aprovisionado. Retire las variables temporales del entorno.");
        }
        catch (PostgresException exception) when (exception.SqlState is "23505" or "23503" or "22023")
        {
            Console.Error.WriteLine(exception.SqlState == "23505"
                ? "No se aprovisionó: ya existe un administrador activo o el correo está en uso."
                : "No se aprovisionó: datos inválidos o el rol Administrador no existe.");
            Environment.ExitCode = 1;
        }
    }

    private static bool EmailValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            return new MailAddress(email.Trim()).Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}