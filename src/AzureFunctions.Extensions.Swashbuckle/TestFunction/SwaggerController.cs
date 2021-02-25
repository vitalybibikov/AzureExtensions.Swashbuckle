using System.Net.Http;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace TestFunction
{
    public static class SwaggerController
    {
        [SwaggerIgnore]
        [FunctionName("SwaggerJson")]
        public static Task<HttpResponseMessage> SwaggerJson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/json")]
            HttpRequestMessage req,
            [SwashBuckleClient] ISwashBuckleClient swashBuckleClient)
        {
            return Task.FromResult(swashBuckleClient.CreateSwaggerJsonDocumentResponse(req));
        }

        [SwaggerIgnore]
        [FunctionName("SwaggerYaml")]
        public static Task<HttpResponseMessage> SwaggerYaml(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/yaml")]
            HttpRequestMessage req,
            [SwashBuckleClient] ISwashBuckleClient swashBuckleClient)
        {
            return Task.FromResult(swashBuckleClient.CreateSwaggerYamlDocumentResponse(req));
        }

        [SwaggerIgnore]
        [FunctionName("SwaggerUi")]
        public static Task<HttpResponseMessage> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/ui")]
            HttpRequestMessage req,
            [SwashBuckleClient] ISwashBuckleClient swashBuckleClient)
        {
            return Task.FromResult(swashBuckleClient.CreateSwaggerUIResponse(req, "swagger/json"));
        }
    }
}