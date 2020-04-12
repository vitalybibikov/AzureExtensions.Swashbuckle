using System.Collections.Generic;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public class Option
    {
        public string Title { get; set; } = "AzureFunctions.Extensions.Swashbuckle";
        public string XmlPath { get; set; }
        public bool AddCodeParameter { get; set; } = true;
        public IEnumerable<OptionDocument> Documents { get; set; } = new List<OptionDocument>();
        public bool PrependOperationWithRoutePrefix { get; set; } = true;
        public bool FillSwaggerBasePathWithRoutePrefix { get; set; } = false;
    }
}