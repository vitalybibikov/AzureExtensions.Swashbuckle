using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle
{
    internal class QueryStringParameterAttributeFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var attributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<QueryStringParamaterAttribute>();

            foreach (var attribute in attributes)
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = attribute.Name,
                    Description = attribute.Description,
                    In = "query",
                    Required = attribute.Required,
                    Type = context.SchemaRegistry.GetOrRegister(attribute.DataType).Type
                });
        }
    }
}
