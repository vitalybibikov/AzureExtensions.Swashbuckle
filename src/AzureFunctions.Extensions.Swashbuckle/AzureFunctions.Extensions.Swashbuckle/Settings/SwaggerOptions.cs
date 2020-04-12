using System.Collections.Generic;
using System.Reflection;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public class SwaggerOptions
    {
        public string Title { get; set; } = Assembly.GetAssembly(typeof(SwaggerOptions)).GetName().Name;
        public string XmlPath { get; set; }
        public bool AddCodeParameter { get; set; } = true;
        public IEnumerable<SwaggerDocument> Documents { get; set; } = new List<SwaggerDocument>();
        public bool PrependOperationWithRoutePrefix { get; set; } = true;
    }
}