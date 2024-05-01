using System.Net.Http;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

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
        public async Task<HttpResponseData> SwaggerJson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/json")]
            HttpRequestData  req)
        {
            return await this.swashBuckleClient.CreateSwaggerJsonDocumentResponse(req);
        }

        [SwaggerIgnore]
        [Function("SwaggerYaml")]
        public async Task<HttpResponseData> SwaggerYaml(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/yaml")]
            HttpRequestData req)
        {
            return await this.swashBuckleClient.CreateSwaggerYamlDocumentResponse(req);
        }

        [SwaggerIgnore]
        [Function("SwaggerUi")]
        public async Task<HttpResponseData> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/ui")]
            HttpRequestData req)
        {
            return await this.swashBuckleClient.CreateSwaggerUIResponse(req, "swagger/json");
        }

        /// <summary>
        /// This is only needed for OAuth2 client. This redirecting document is normally served
        /// as a static content. Functions don't provide this out of the box, so we serve it here.
        /// Don't forget to set OAuth2RedirectPath configuration option to reflect this route.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="swashBuckleClient"></param>
        /// <returns></returns>
        [SwaggerIgnore]
        [Function("SwaggerOAuth2Redirect")]
        public async Task<HttpResponseData> SwaggerOAuth2Redirect(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "swagger/oauth2-redirect")]
            HttpRequestData req)
        {
            return await this.swashBuckleClient.CreateSwaggerOAuth2RedirectResponse(req);
        }
    }
}
