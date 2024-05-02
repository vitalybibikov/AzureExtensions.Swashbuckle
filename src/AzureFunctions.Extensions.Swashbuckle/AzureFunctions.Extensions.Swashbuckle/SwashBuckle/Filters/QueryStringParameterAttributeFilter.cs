using System.Linq;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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

                    if (attribute != null)
                    {
                        switch (attribute.Example)
                        {
                            case OpenApiNull _:
                                /* ignore */
                                break;

                            default:
                                // set both examples
                                apiParameter.Schema.Example = attribute.Example;
                                apiParameter.Example = apiParameter.Schema.Example;
                                break;
                        }
                    }

                    operation.Parameters.Add(apiParameter);
                }
            }
        }
    }
}
