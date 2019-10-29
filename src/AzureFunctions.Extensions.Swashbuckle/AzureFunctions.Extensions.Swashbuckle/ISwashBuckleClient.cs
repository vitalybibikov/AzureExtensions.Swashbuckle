using System.IO;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public interface ISwashBuckleClient
    {
        Stream GetSwaggerDocument(string host, string documentName = "v1");

        Stream GetSwaggerUi(string swaggerUrl);

        string RoutePrefix { get; }
    }
}
