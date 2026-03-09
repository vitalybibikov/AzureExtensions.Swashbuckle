using System;
using System.Collections.Generic;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestFunction.CustomFilterExample
{
    public class RemoveSchemasFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var keysToRemove = new List<string>(swaggerDoc.Components.Schemas.Keys);
            foreach (var key in keysToRemove)
            {
                swaggerDoc.Components.Schemas.Remove(key);
            }
        }
    }
}
