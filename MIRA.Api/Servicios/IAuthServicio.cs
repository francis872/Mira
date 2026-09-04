namespace MIRA.Api.Servicios;

public interface IAuthServicio
{
    Task<AuthResultado> LoginAsync(string correo, string password, CancellationToken cancellationToken);
}
