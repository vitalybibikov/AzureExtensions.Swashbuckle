using System.IO;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle
{
    internal class SwashBuckleClient : ISwashBuckleClient
    {
        private readonly SwashbuckleConfig _config;

        public SwashBuckleClient(SwashbuckleConfig config)
        {
            _config = config;
        }

        public Stream GetSwaggerJsonDocument(string host, string documentName = "v1")
        {
            return _config.GetSwaggerJsonDocument(host, documentName);
        }

        public Stream GetSwaggerYamlDocument(string host, string documentName = "v1")
        {
            return _config.GetSwaggerYamlDocument(host, documentName);
        }

        public Stream GetSwaggerUi(string swaggerUrl)
        {
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            writer.Write(_config.GetSwaggerUIContent(swaggerUrl));
            writer.Flush();

            memoryStream.Position = 0;
            return memoryStream;
        }

        public string RoutePrefix => _config.RoutePrefix;
    }
}