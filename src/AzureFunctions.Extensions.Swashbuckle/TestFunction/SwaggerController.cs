using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;

namespace TestFunction
{
    public static class SwaggerController
    {
        [SwaggerIgnore]
        [Function("SwaggerJson")]
        public static Task<HttpResponseData> SwaggerJson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/json")]
            HttpRequestData req)
        {
            var swashBuckleClient = new SwashBuckleClient(null);
            return Task.FromResult(swashBuckleClient.CreateSwaggerJsonDocumentResponse(req));
        }

     
        /// This is only needed for OAuth2 client. This redirecting document is normally served
        /// as a static content. Functions don't provide this out of the box, so we serve it here.
        /// Don't forget to set OAuth2RedirectPath configuration option to reflect this route.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="swashBuckleClient"></param>
        /// <returns></returns>
        [SwaggerIgnore]
        [Function("SwaggerOAuth2Redirect")]
        public static Task<HttpResponseData> SwaggerOAuth2Redirect(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "swagger/oauth2-redirect")]
            HttpRequestData req)
        {
            var swashBuckleClient = new SwashBuckleClient(null);
            return Task.FromResult(swashBuckleClient.CreateSwaggerOAuth2RedirectResponse(req));
        }
    }
}