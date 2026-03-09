using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Net.Http;

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

            using var stream = await client.GetSwaggerJsonDocumentAsync(host, documentName);
            using var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();

            var response = requestData.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(document);

            return response;
        }

        public static async Task<HttpResponseData> CreateSwaggerYamlDocumentResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestData,
            string documentName = "v1")
        {
            var host = GetBaseUri(requestData);

            using var stream = await client.GetSwaggerYamlDocumentAsync(host, documentName);
            using var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();

            var response = requestData.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/x-yaml; charset=utf-8");
            await response.WriteStringAsync(document);

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
            using var stream = client.GetSwaggerUi($"{host}{routePrefix}/{documentRoute}");
            using var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();

            var response = requestData.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await response.WriteStringAsync(document);
            return response;
        }

        public static async Task<HttpResponseData> CreateSwaggerOAuth2RedirectResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestData)
        {
            using var stream = client.GetSwaggerOAuth2Redirect();
            using var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();

            var response = requestData.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await response.WriteStringAsync(document);
            return response;
        }

        private static string GetBaseUri(HttpRequestData requestData)
        {
            var host = requestData.Url.Host;
            var port = requestData.Url.Port;
            var scheme = requestData.Url.Scheme;

            var hostWithPort = (port == 80 && scheme == "http") || (port == 443 && scheme == "https") ? host : $"{host}:{port}";

            var fullHost = $"{scheme}://{hostWithPort}";
            return fullHost;
        }
    }
}
