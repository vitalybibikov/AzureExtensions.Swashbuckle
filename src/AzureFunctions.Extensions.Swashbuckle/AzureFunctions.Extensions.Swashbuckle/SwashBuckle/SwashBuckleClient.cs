using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle
{
    public sealed class SwashBuckleClient : ISwashBuckleClient
    {
        private readonly SwashbuckleConfig _config;

        public SwashBuckleClient(SwashbuckleConfig config)
        {
            _config = config;
        }

        public async Task<Stream> GetSwaggerJsonDocumentAsync(string host, string documentName = "v1")
        {
            return await _config.GetSwaggerJsonDocumentAsync(host, documentName);
        }

        public async Task<Stream> GetSwaggerYamlDocumentAsync(string host, string documentName = "v1")
        {
            return await _config.GetSwaggerYamlDocumentAsync(host, documentName);
        }

        public Stream GetSwaggerOAuth2Redirect()
        {
            var content = _config.GetSwaggerOAuth2RedirectContent();
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
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
