using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIRA.Api.Peticiones;
using MIRA.Api.Servicios;

namespace MIRA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("/api/health")]
        [AllowAnonymous]
        public IActionResult HealthCheck([FromServices] Configuracion.IDbConnectionFactory connectionFactory)
        {
            try
            {
                using var conn = connectionFactory.CreateConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(1) FROM usuario;";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return Ok(new
                {
                    status = "Healthy",
                    database = "Connected",
                    usuariosRegistrados = count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "DatabaseConnectionError",
                    tipoError = ex.GetType().Name,
                    detalle = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest peticion)
        {
            try
            {
                var resultado = await _authService.AutenticarAsync(peticion);

                if (resultado == null)
                {
                    return Unauthorized(new
                    {
                        estado = 401,
                        mensaje = "Credenciales inválidas",
                        detalle = "El nombre de usuario o la contraseña no coinciden."
                    });
                }

                return Ok(new
                {
                    estado = 200,
                    mensaje = "Autenticación exitosa",
                    token = resultado.Token,
                    usuario = resultado.Usuario
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estado = 500,
                    mensaje = "Error al autenticar",
                    detalle = ex.Message,
                    tipo = ex.GetType().Name
                });
            }
        }
    }
}
