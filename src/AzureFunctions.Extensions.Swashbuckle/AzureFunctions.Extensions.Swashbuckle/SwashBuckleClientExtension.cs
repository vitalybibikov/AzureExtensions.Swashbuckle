using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleClientExtension
    {
        public static HttpResponseMessage CreateSwaggerDocumentResponse(
            this ISwashBuckleClient client,
            HttpRequestMessage requestMessage,
            string documentName = "v1")
        {
            var host = GetBaseUri(client, requestMessage);

            var stream = client.GetSwaggerDocument(host, documentName);
            var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = requestMessage,
                Content = new StringContent(document, Encoding.UTF8, "application/json")
            };

            return response;
        }

        public static HttpResponseMessage CreateSwaggerUIResponse(
            this ISwashBuckleClient client,
            HttpRequestMessage requestMessage,
            string documentRoute)
        {
            var routePrefix = string.IsNullOrEmpty(client.RoutePrefix)
                ? string.Empty
                : $"/{client.RoutePrefix}";

            var host = GetBaseUri(client, requestMessage);
            var stream = client.GetSwaggerUi($"{host}{routePrefix}/{documentRoute}");

            var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = requestMessage,
                Content = new StringContent(document, Encoding.UTF8, "text/html")
            };

            return result;
        }


        private static string GetBaseUri(ISwashBuckleClient client, HttpRequestMessage requestMessage)
        {
            var authority = requestMessage.RequestUri.Authority.TrimEnd('/');
            var scheme = requestMessage.RequestUri.Scheme;
            var host = $"{scheme}://{authority}";

            return host;
        }
    }
}