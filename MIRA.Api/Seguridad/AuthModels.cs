namespace MIRA.Api.Seguridad;
public sealed record LoginRequest(string Correo, string Password);
public sealed record CrearUsuarioRequest(string Correo, string Password, int[] Roles);
public sealed record ActualizarUsuarioRequest(string Correo, int[] Roles);
public sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc);
