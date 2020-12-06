using System.Linq;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    internal class QueryStringParameterAttributeFilter : IOperationFilter
    {
        private readonly ISchemaGenerator schemaGenerator;

        public QueryStringParameterAttributeFilter(ISchemaGenerator schemaGenerator)
        {
            this.schemaGenerator = schemaGenerator;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.DeclaringType != null)
            {
                var attributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                    .Union(context.MethodInfo.GetCustomAttributes(true))
                    .OfType<QueryStringParameterAttribute>();

                foreach (var attribute in attributes)
                {
                    var attributeTypeName = new OpenApiSchema {Type = "string"};

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = attribute.Name,
                        Description = attribute.Description,
                        In = ParameterLocation.Query,
                        Required = attribute.Required,
                        Schema = schemaGenerator.GenerateSchema(attribute?.DataType, new SchemaRepository())
                    });
                }
            }
        }
    }
}