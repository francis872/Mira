namespace MIRA.Api.Seguridad;

public sealed class AuthServiceException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}