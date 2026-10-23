using Npgsql;

namespace MIRA.Api.Excepciones;

/// <summary>Traduce fallos de persistencia sin publicar SQL, trazas ni datos sensibles.</summary>
public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception error) when (!context.Response.HasStarted)
        {
            var (status, message) = error switch
            {
                MIRA.Api.Servicios.ValidacionInvestigacionException validation => (400, validation.Message),
                MIRA.Api.Servicios.RecursoInvestigacionNoEncontradoException => (404, "El módulo solicitado no existe."),
                PostgresException { SqlState: "23503" } => (409, "No se puede completar la operación por una relación con otro registro. Revise las referencias seleccionadas."),
                PostgresException { SqlState: "23505" } => (409, "Ya existe un registro con esos datos. Revise la información ingresada."),
                PostgresException { SqlState: "23502" or "22023" } => (400, "Faltan datos obligatorios o contienen valores inválidos. Revise el formulario."),
                PostgresException { SqlState: "22001" } => (400, "Un campo supera la longitud permitida. Reduzca el texto ingresado."),
                PostgresException { SqlState: "23514" or "22007" or "22008" or "22P02" } => (400, "Revise el formato, las fechas y las opciones seleccionadas en el formulario."),
                _ => (500, "No fue posible completar la operación. Intente de nuevo; si el problema continúa, contacte al responsable de la plataforma.")
            };
            // No registrar el texto de la excepción: puede contener SQL o datos del usuario.
            logger.LogWarning("Solicitud fallida. Estado {Status}; tipo {ErrorType}; referencia {TraceId}",
                status, error.GetType().Name, context.TraceIdentifier);
            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new { mensaje = message });
        }
    }
}
