using System.IO;

namespace AzureFunctions.Extensions.Swashbuckle
{
    internal class SwashBuckleClient : ISwashBuckleClient
    {
        private readonly SwashbuckleConfig _config;

        public SwashBuckleClient(SwashbuckleConfig config)
        {
            _config = config;
        }

        public Stream GetSwaggerDocument(string host, string documentName = "v1")
        {
            return _config.GetSwaggerDocument(documentName, host);
        }

        public Stream GetSwaggerUi(string swaggerUrl)
        {
            var mem = new MemoryStream();
            var writer = new StreamWriter(mem);
            writer.Write(_config.GetSwaggerUIContent(swaggerUrl));
            writer.Flush();
            mem.Position = 0;
            return mem;
        }

        public string RoutePrefix => _config.RoutePrefix;
    }
}
