using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FitLead.Api.Swagger
{
    public sealed class UserContextHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            if (operation.Parameters.Any(p => p.Name == "X-Identity-User-Id"))
                return;

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Identity-User-Id",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Temporary dev backdoor. Use JWT login flow for /auth endpoints.",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}
