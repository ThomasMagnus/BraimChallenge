using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BraimChallenge.Helpers
{
    public class CustomHeaderSwaggerAttribute : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();

            if (context.ApiDescription.ActionDescriptor.RouteValues.TryGetValue("registration", out string controllerName))
            {
                return;
            }

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                }
            });
        }
    }
}
