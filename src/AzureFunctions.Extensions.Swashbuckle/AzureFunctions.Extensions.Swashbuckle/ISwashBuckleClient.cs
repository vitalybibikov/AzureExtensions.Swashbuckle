using System.IO;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public interface ISwashBuckleClient
    {
        Stream GetSwaggerDocument();

        Stream GetSwaggerUi(string swaggerUrl);

        string RoutePrefix { get; }
    }
}
