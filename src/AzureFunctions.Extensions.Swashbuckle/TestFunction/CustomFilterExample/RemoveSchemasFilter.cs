using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestFunction.CustomFilterExample
{
    public class RemoveSchemasFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (KeyValuePair<string, OpenApiSchema> item in swaggerDoc.Components.Schemas)
            {
                swaggerDoc.Components.Schemas.Remove(item.Key);
            }
        }
    }
}
