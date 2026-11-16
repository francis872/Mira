using System.Data.Common;
using Dapper;

namespace MIRA.Api.Configuracion;

public static class Readiness
{
    public static async Task<IResult> Check(IDbConnectionFactory factory, HttpContext context)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        deadline.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            using var connection = factory.CreateConnection();
            await ((DbConnection)connection).OpenAsync(deadline.Token);
            var ready = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                "SELECT to_regclass('public.sesion_acceso') IS NOT NULL", commandTimeout:5, cancellationToken:deadline.Token));
            return ready ? Results.Ok(new { status="ready" }) : Results.StatusCode(503);
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            // No exception text, DSN, SQL, host or credentials in public output.
            return Results.StatusCode(503);
        }
    }
}
