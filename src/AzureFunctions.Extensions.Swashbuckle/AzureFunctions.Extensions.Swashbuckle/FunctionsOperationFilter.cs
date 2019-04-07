using System;
using System.Collections.Generic;
using System.Text;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle
{
    internal class FunctionsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();

            foreach (var customAttribute in context.MethodInfo.GetCustomAttributes(typeof(RequestHttpHeaderAttribute), false))
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = (customAttribute as RequestHttpHeaderAttribute).HeaderName,
                    In = "header",
                    Type = "string",
                    Required = (customAttribute as RequestHttpHeaderAttribute).IsRequired
                });
            }

            foreach (var customAttribute in context.MethodInfo.DeclaringType.GetCustomAttributes(typeof(RequestHttpHeaderAttribute), false))
            {
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = (customAttribute as RequestHttpHeaderAttribute).HeaderName,
                    In = "header",
                    Type = "string",
                    Required = (customAttribute as RequestHttpHeaderAttribute).IsRequired
                });
            }
        }
    }
}
