using System;
using System.Collections.Generic;
using System.Text;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle
{
    internal class FunctionsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            foreach (var customAttribute in context.MethodInfo.GetCustomAttributes(typeof(RequestHttpHeaderAttribute), false))
            {
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = (customAttribute as RequestHttpHeaderAttribute).HeaderName,
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Type = "string" },
                    Required = (customAttribute as RequestHttpHeaderAttribute).IsRequired
                });
            }

            foreach (var customAttribute in context.MethodInfo.DeclaringType.GetCustomAttributes(typeof(RequestHttpHeaderAttribute), false))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = (customAttribute as RequestHttpHeaderAttribute).HeaderName,
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Type = "string" },
                    Required = (customAttribute as RequestHttpHeaderAttribute).IsRequired
                });
            }
        }
    }
}
