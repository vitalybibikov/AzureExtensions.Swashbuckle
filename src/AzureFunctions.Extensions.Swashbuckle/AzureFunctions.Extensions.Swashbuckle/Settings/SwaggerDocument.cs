using System;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public class SwaggerDocument
    {
        public string Name { get; set; } = "v1";
        public string Title { get; set; } = "Swashbuckle";
        public string Version { get; set; } = "v1";
        public string Description { get; set; } = "Swagger document by Swashbuckle";
    }
}
