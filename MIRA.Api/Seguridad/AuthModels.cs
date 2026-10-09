namespace MIRA.Api.Seguridad;

/// <summary>Credentials submitted to obtain a bearer token.</summary>
public sealed record LoginRequest(string Correo, string Password);

/// <summary>Payload for creating a user and assigning active roles.</summary>
public sealed record CrearUsuarioRequest(string Correo, string Password, int[] Roles);

/// <summary>Payload for updating a user's email and complete role assignment.</summary>
public sealed record ActualizarUsuarioRequest(string Correo, int[] Roles);

/// <summary>Bearer token and the roles granted to its identity.</summary>
public sealed record LoginResponse(string AccessToken, string TokenType, DateTimeOffset ExpiresAtUtc, IReadOnlyList<string> Roles);

/// <summary>Safe user representation returned by administrative endpoints.</summary>
public sealed record UsuarioAdminResponse(long Id, string Correo, bool Activo, IReadOnlyList<RolItem> Roles);
