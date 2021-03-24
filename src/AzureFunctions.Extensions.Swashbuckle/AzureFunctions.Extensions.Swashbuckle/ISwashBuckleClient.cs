using System.IO;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public interface ISwashBuckleClient
    {
        string RoutePrefix { get; }
        Stream GetSwaggerJsonDocument(string host, string documentName = "v1");
        Stream GetSwaggerYamlDocument(string host, string documentName = "v1");
        Stream GetSwaggerUi(string swaggerUrl);
        Stream GetSwaggerOAuth2Redirect();
    }
}