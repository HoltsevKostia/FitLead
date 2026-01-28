using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FitLead.Api.Swagger
{
    public sealed class UserContextHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            // не дублюємо, якщо вже є
            if (operation.Parameters.Any(p => p.Name == "X-User-Id"))
                return;

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-User-Id",
                In = ParameterLocation.Header,
                Required = false, // MVP
                Description = "Current user id (MVP/dev only). Will be replaced by JWT claims post-MVP.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid"
                }
            });
        }
    }
}
