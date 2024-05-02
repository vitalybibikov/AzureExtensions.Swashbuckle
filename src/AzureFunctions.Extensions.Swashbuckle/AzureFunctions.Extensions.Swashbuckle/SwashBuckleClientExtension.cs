using Microsoft.Azure.Functions.Worker.Http;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleClientExtension
    {
        public static async Task<HttpResponseData> CreateSwaggerJsonDocumentResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestData,
            string documentName = "v1")
        {
            var host = GetBaseUri(requestData);

            var stream = client.GetSwaggerJsonDocument(host, documentName);
            var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();

            var response = requestData.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(document, CancellationToken.None, Encoding.UTF8);

            //response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response;
        }

        public static async Task<HttpResponseData> CreateSwaggerYamlDocumentResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestData,
            string documentName = "v1")
        {
            var host = GetBaseUri(requestData);

            var stream = client.GetSwaggerYamlDocument(host, documentName);
            var reader = new StreamReader(stream);

            var response = requestData.CreateResponse(HttpStatusCode.OK);

            var document = await reader.ReadToEndAsync();
            await response.WriteStringAsync(document, CancellationToken.None, Encoding.UTF8);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            return response;
        }

        public static async Task<HttpResponseData> CreateSwaggerUIResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestData,
            string documentRoute)
        {
            var routePrefix = string.IsNullOrEmpty(client.RoutePrefix)
                ? string.Empty
                : $"/{client.RoutePrefix}";

            var host = GetBaseUri(requestData);
            var stream = client.GetSwaggerUi($"{host}{routePrefix}/{documentRoute}");

            var response = requestData.CreateResponse(HttpStatusCode.OK);
            using var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();
            await response.WriteStringAsync(document, CancellationToken.None, Encoding.UTF8);
            //response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            return response;
        }

        public static async Task<HttpResponseData> CreateSwaggerOAuth2RedirectResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestData)
        {
            var stream = client.GetSwaggerOAuth2Redirect();

            var response = requestData.CreateResponse(HttpStatusCode.OK);
            using var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();
            await response.WriteStringAsync(document, CancellationToken.None, Encoding.UTF8);
            //response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            return response;
        }

        private static string GetBaseUri(HttpRequestData requestData)
        {
            var host = requestData.Url.Host;
            var port = requestData.Url.Port;  // Get the port
            var scheme = requestData.Url.Scheme;

            var hostWithPort = (port == 80 && scheme == "http") || (port == 443 && scheme == "https") ? host : $"{host}:{port}";

            var fullHost = $"{scheme}://{hostWithPort}";
            return fullHost;
        }
    }
}
