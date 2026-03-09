using System.IO;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public interface ISwashBuckleClient
    {
        string RoutePrefix { get; }
        Task<Stream> GetSwaggerJsonDocumentAsync(string host, string documentName = "v1");
        Task<Stream> GetSwaggerYamlDocumentAsync(string host, string documentName = "v1");
        Stream GetSwaggerUi(string swaggerUrl);
        Stream GetSwaggerOAuth2Redirect();
    }
}
