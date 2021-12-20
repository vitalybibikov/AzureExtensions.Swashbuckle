using System.IO;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle
{
    public interface ISwashbuckleConfig
    {
        Stream GetSwaggerJsonDocument(string host, string documentName = "v1");
        string GetSwaggerOAuth2RedirectContent();
        string GetSwaggerUIContent(string swaggerUrl);
        Stream GetSwaggerYamlDocument(string host, string documentName = "v1");
        void Initialize(IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider);
        string RoutePrefix { get; }
    }
}
