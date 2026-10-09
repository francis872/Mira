using System.Net.Mail;
using MIRA.Api.Configuracion;

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

        var workFactor = configuration.GetValue("BCrypt:WorkFactor", 12);
        if (workFactor is < 10 or > 14)
        {
            throw new InvalidOperationException("BCrypt:WorkFactor debe estar entre 10 y 14.");
        }

        var repository = new AuthRepository(new DbConnectionFactory(configuration));
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        await repository.BootstrapAdminAsync(normalizedEmail!, hash);
        Console.WriteLine("Primer administrador aprovisionado. Retire las variables temporales del entorno.");
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