using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
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
                    if (attribute.DataType != null && attribute.DataType.GetTypeInfo().IsClass)
                    {
                        //If data type of query string param is class we gonna go thru the props of class
                        // in future maybe to improve this to be more generic 
                        foreach (var prop in attribute.DataType.GetProperties())
                        {
                            if(prop.PropertyType.GetTypeInfo().IsClass)
                            {
                                foreach(var pro in prop.PropertyType.GetProperties())
                                {
                                    operation.Parameters.Add(GetOpenApiParameter(pro));
                                }
                            }
                            else
                            {
                                operation.Parameters.Add(GetOpenApiParameter(prop));
                            }
                        }
                    }
                    else
                    {
                        operation.Parameters.Add(GetOpenApiParameter(attribute));
                    }
                }
            }
        }

        private OpenApiParameter GetOpenApiParameter(QueryStringParameterAttribute attribute)
        {
            var apiParameter = new OpenApiParameter
            {
                Name = attribute.Name,
                Description = attribute.Description,
                In = ParameterLocation.Query,
                Required = attribute.Required,
                Schema = schemaGenerator.GenerateSchema(attribute?.DataType, new SchemaRepository())
            };

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
            return apiParameter;
        }

        private OpenApiParameter GetOpenApiParameter(PropertyInfo propertyInfo)
        {
            var attribute = propertyInfo?.DeclaringType?.GetCustomAttributes(true).OfType<JsonPropertyNameAttribute>().FirstOrDefault();

            var apiParameter = new OpenApiParameter
            {
                Name = attribute?.Name ?? propertyInfo?.Name,
                In = ParameterLocation.Query,
                Required = false,
            };

            return apiParameter;
        }
    }
}
