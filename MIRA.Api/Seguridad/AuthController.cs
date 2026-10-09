using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MIRA.Api.Seguridad;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
 private readonly IAuthService _authService;
 public AuthController(IAuthService authService) => _authService = authService;

 [AllowAnonymous]
 [HttpPost("login")]
 public async Task<IActionResult> Login(LoginRequest request)
 {
  if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Password))
   return Unauthorized(new { mensaje = "Credenciales inválidas." });

  var result = await _authService.LoginAsync(request);
  return result is null ? Unauthorized(new { mensaje = "Credenciales inválidas." }) : Ok(result);
 }

 [Authorize(Roles="Administrador")]
 [HttpGet("roles")]
 public async Task<IActionResult> Roles()
 {
  try { return Ok(await _authService.RolesActivosAsync()); }
  catch (AuthServiceException exception) { return Error(exception); }
 }

 [Authorize(Roles="Administrador")]
 [HttpGet("usuarios")]
 public async Task<IActionResult> Listar()
 {
  try { return Ok(await _authService.ListarUsuariosAsync()); }
  catch (AuthServiceException exception) { return Error(exception); }
 }

 [Authorize(Roles="Administrador")]
 [HttpGet("usuarios/{id:long}")]
 public async Task<IActionResult> Consultar(long id)
 {
  try
  {
   var result = await _authService.ConsultarUsuarioAsync(id);
   return result is null ? NotFound() : Ok(result);
  }
  catch (AuthServiceException exception) { return Error(exception); }
 }

 [Authorize(Roles="Administrador")]
 [HttpPost("usuarios")]
 public async Task<IActionResult> Crear(CrearUsuarioRequest request)
 {
  try
  {
    var created = await _authService.CrearUsuarioAsync(request);
    return CreatedAtAction(nameof(Consultar), new { id = created.Id }, created);
  }
    catch (AuthServiceException exception) { return Error(exception); }
 }

 [Authorize(Roles="Administrador")]
 [HttpPut("usuarios/{id:long}")]
 public async Task<IActionResult> Actualizar(long id,ActualizarUsuarioRequest request)
 {
  try { return Ok(await _authService.ActualizarUsuarioAsync(id, request)); }
  catch (AuthServiceException exception) { return Error(exception); }
 }

 [Authorize(Roles="Administrador")]
 [HttpDelete("usuarios/{id:long}")]
 public async Task<IActionResult> Inactivar(long id)
 {
  try { return await _authService.InactivarUsuarioAsync(id) ? NoContent() : NotFound(); }
  catch (AuthServiceException exception) { return Error(exception); }
 }

 private IActionResult Error(AuthServiceException exception)
     => StatusCode(exception.StatusCode, new { mensaje = exception.Message });
}
