using System.Text.Json;

namespace MIRA.Api.Excepciones;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException apiException)
        {
            logger.LogWarning(apiException, "Handled API exception: {Message}", apiException.Message);
            context.Response.StatusCode = apiException.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = apiException.Message }));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled server error");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Internal server error" }));
        }
    }
}
