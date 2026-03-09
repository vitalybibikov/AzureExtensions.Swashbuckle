using System.Collections.Generic;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    internal class FunctionsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<IOpenApiParameter>();

            foreach (var customAttribute in context.MethodInfo.GetCustomAttributes(typeof(RequestHttpHeaderAttribute), false))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = (customAttribute as RequestHttpHeaderAttribute)!.HeaderName,
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                    Required = (customAttribute as RequestHttpHeaderAttribute)!.IsRequired
                });
            }

            foreach (var customAttribute in context.MethodInfo.DeclaringType!.GetCustomAttributes(
                typeof(RequestHttpHeaderAttribute), false))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = (customAttribute as RequestHttpHeaderAttribute)!.HeaderName,
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                    Required = (customAttribute as RequestHttpHeaderAttribute)!.IsRequired
                });
            }
        }
    }
}
