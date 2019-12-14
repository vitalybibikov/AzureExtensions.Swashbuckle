namespace AzureFunctions.Extensions.Swashbuckle
{
    public class Option
    {

        public string Title { get; set; } = "AzureFunctions.Extensions.Swashbuckle";
        public string XmlPath { get; set; }
        public bool AddCodeParamater { get; set; } = true;

        public OptionDocument[] Documents { get; set; } = { };

        public bool PrepandOperationWithRoutePrefix { get; set; } = true;

        public bool FillSwaggerBasePathWithRoutePrefix { get; set; } = false;
    }

    public class OptionDocument
    {
        public string Name { get; set; } = "v1";
        public string Title { get; set; } = "Swashbuckle";

        public string Version { get; set; } = "v1";

        public string Description { get; set; } = "Swagger document by Swashbuckle";
    }
}
