using System.Linq;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    internal class QueryStringParameterAttributeFilter : IOperationFilter
    {
        private readonly ISchemaGenerator schemaGenerator;

        public QueryStringParameterAttributeFilter(ISchemaGenerator schemaGenerator)
        {
            this.schemaGenerator = schemaGenerator ?? throw new ArgumentNullException(nameof(schemaGenerator));
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
                    var apiParameter = new OpenApiParameter
                    {
                        Name = attribute.Name,
                        Description = attribute.Description,
                        In = ParameterLocation.Query,
                        Required = attribute.Required,
                        Schema = this.schemaGenerator.GenerateSchema(attribute?.DataType, new SchemaRepository())
                    };

                    if (attribute != null && attribute.Example != null)
                    {
                        if (apiParameter.Schema is OpenApiSchema concreteSchema)
                        {
                            concreteSchema.Example = attribute.Example;
                        }

                        apiParameter.Example = attribute.Example;
                    }

                    operation.Parameters?.Add(apiParameter);
                }
            }
        }
    }
}
