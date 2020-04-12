using System.Collections.Generic;
using System.Reflection;

namespace AzureFunctions.Extensions.Swashbuckle.Settings
{
    public class SwaggerDocOptions
    {
        public string Title { get; set; } = Assembly.GetAssembly(typeof(SwaggerDocOptions)).GetName().Name;

        public string XmlPath { get; set; }

        public bool AddCodeParameter { get; set; } = true;

        public IEnumerable<SwaggerDocument> Documents { get; set; } = new List<SwaggerDocument>();

        public bool PrependOperationWithRoutePrefix { get; set; } = true;

        public bool SerializeAsV2 { get; set; } = false;
    }
}