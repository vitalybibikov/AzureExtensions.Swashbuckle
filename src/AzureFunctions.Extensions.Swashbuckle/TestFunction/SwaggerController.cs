using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace TestFunction
{
    public class SwaggerController
    {
        private readonly ISwashBuckleClient swashBuckleClient;

        public SwaggerController(ISwashBuckleClient swashBuckleClient)
        {
            this.swashBuckleClient = swashBuckleClient;
        }

        [SwaggerIgnore]
        [Function("SwaggerJson")]
        public async Task<IActionResult> SwaggerJson(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Swagger/json")]
            HttpRequest req)
        {
            return await this.swashBuckleClient.CreateSwaggerJsonDocumentResult(req);
        }

        [SwaggerIgnore]
        [Function("SwaggerYaml")]
        public async Task<IActionResult> SwaggerYaml(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Swagger/yaml")]
            HttpRequest req)
        {
            return await this.swashBuckleClient.CreateSwaggerYamlDocumentResult(req);
        }

        [SwaggerIgnore]
        [Function("SwaggerUi")]
        public async Task<IActionResult> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Swagger/ui")]
            HttpRequest req)
        {
            return await this.swashBuckleClient.CreateSwaggerUIResult(req, "swagger/json");
        }

        [SwaggerIgnore]
        [Function("SwaggerOAuth2Redirect")]
        public async Task<IActionResult> SwaggerOAuth2Redirect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/oauth2-redirect")]
            HttpRequest req)
        {
            return await this.swashBuckleClient.CreateSwaggerOAuth2RedirectResult();
        }
    }
}
