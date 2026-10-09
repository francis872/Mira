using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
namespace MIRA.Api.Seguridad;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
 private readonly IAuthRepository _users;
 private readonly IConfiguration _config;
 public AuthController(IAuthRepository users,IConfiguration config){_users=users;_config=config;}

 [AllowAnonymous]
 [HttpPost("login")]
 public async Task<IActionResult> Login(LoginRequest request)
 {
  if (string.IsNullOrWhiteSpace(request.Correo)||string.IsNullOrWhiteSpace(request.Password))
   return Unauthorized(new{mensaje="Credenciales inválidas."});
  var user=await _users.BuscarPorCorreoAsync(request.Correo);
  if (user is null || !user.Activo || !BCrypt.Net.BCrypt.Verify(request.Password,user.PasswordHash))
   return Unauthorized(new{mensaje="Credenciales inválidas."});
  var roles=await _users.RolesUsuarioAsync(user.Id);
  var secret=_config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret no configurado");
  var issuer=_config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer no configurado");
  var audience=_config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado");
  var expires=DateTime.UtcNow.AddMinutes(60);
  var claims=new List<Claim>{new(ClaimTypes.NameIdentifier,user.Id.ToString()),new(ClaimTypes.Name,user.Correo)};
  claims.AddRange(roles.Select(r=>new Claim(ClaimTypes.Role,r.Nombre)));
  var token=new JwtSecurityToken(issuer,audience,claims,expires:expires,
   signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),SecurityAlgorithms.HmacSha256));
  return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token),"Bearer",expires));
 }

 [Authorize(Roles="Administrador")]
 [HttpGet("roles")]
 public async Task<IActionResult> Roles()=>Ok(await _users.RolesActivosAsync());

 [Authorize(Roles="Administrador")]
 [HttpGet("usuarios")]
 public async Task<IActionResult> Listar()=>Content(await _users.ListarAsync(),"application/json");

 [Authorize(Roles="Administrador")]
 [HttpGet("usuarios/{id:long}")]
 public async Task<IActionResult> Consultar(long id)
 {
  var result=await _users.ConsultarAsync(id);
  return result is null?NotFound():Content(result,"application/json");
 }

 [Authorize(Roles="Administrador")]
 [HttpPost("usuarios")]
 public async Task<IActionResult> Crear(CrearUsuarioRequest request)
 {
  if (!Validar(request.Correo,request.Roles)||request.Password?.Length<12)
   return BadRequest(new{mensaje="Correo, roles o contraseña inválidos (mínimo 12 caracteres)."});
  try
  {
   var hash=BCrypt.Net.BCrypt.HashPassword(request.Password,workFactor:12);
   return Content(await _users.CrearAsync(request.Correo,hash,request.Roles),"application/json");
  }
  catch(PostgresException ex) when(ex.SqlState is "23505" or "23503" or "22023")
  {return Conflict(new{mensaje="Usuario o roles inválidos o duplicados."});}
 }

 [Authorize(Roles="Administrador")]
 [HttpPut("usuarios/{id:long}")]
 public async Task<IActionResult> Actualizar(long id,ActualizarUsuarioRequest request)
 {
  if (!Validar(request.Correo,request.Roles))return BadRequest();
  try{return Content((await _users.ActualizarAsync(id,request.Correo,request.Roles))!,"application/json");}
  catch(PostgresException ex) when(ex.SqlState=="P0002"){return NotFound();}
  catch(PostgresException ex) when(ex.SqlState is "23505" or "23503" or "22023"){return Conflict();}
 }

 [Authorize(Roles="Administrador")]
 [HttpDelete("usuarios/{id:long}")]
 public async Task<IActionResult> Inactivar(long id)
   => await _users.InactivarAsync(id)?NoContent():NotFound();

 private static bool Validar(string? correo,int[]? roles)
   => !string.IsNullOrWhiteSpace(correo) && correo.Contains('@')
      && roles is { Length: > 0 } && roles.All(x=>x>0);
}
