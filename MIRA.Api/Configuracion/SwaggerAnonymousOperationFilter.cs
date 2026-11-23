using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MIRA.Api.Configuracion;

public sealed class SwaggerAnonymousOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            operation.Security = [];
    }
}
