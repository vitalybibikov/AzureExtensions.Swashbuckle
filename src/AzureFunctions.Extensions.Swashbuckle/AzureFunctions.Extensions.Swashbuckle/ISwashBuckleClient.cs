using System.IO;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public interface ISwashBuckleClient
    {
        string RoutePrefix { get; }
        Stream GetSwaggerDocument(string host, string documentName = "v1");
        Stream GetSwaggerUi(string swaggerUrl);
    }
}