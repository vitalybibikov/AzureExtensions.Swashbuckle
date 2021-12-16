using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;


namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleClientExtension
    {
        public static HttpResponseData CreateSwaggerJsonDocumentResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestMessage,
            string documentName = "v1")
        {
            var host = GetBaseUri(client, requestMessage);

            var stream = client.GetSwaggerJsonDocument(host, documentName);
            var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            var result = requestMessage.CreateResponse(HttpStatusCode.OK);
            result.WriteString(document);
            result.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return result;

        }

        public static HttpResponseData CreateSwaggerYamlDocumentResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestMessage,
            string documentName = "v1")
        {
            var host = GetBaseUri(client, requestMessage);

            var stream = client.GetSwaggerYamlDocument(host, documentName);
            var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            var result = requestMessage.CreateResponse(HttpStatusCode.OK);
            result.WriteString(document);
            result.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return result;
        }

        public static HttpResponseData CreateSwaggerUIResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestMessage,
            string documentRoute)
        {
            var routePrefix = string.IsNullOrEmpty(client.RoutePrefix)
                ? string.Empty
                : $"/{client.RoutePrefix}";

            var host = GetBaseUri(client, requestMessage);
            var stream = client.GetSwaggerUi($"{host}{routePrefix}/{documentRoute}");

            var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            var result = requestMessage.CreateResponse(HttpStatusCode.OK);
            result.WriteString(document);
            result.Headers.Add("Content-Type", "text/html; charset=utf-8");
            return result;
        }

        public static HttpResponseData CreateSwaggerOAuth2RedirectResponse(
            this ISwashBuckleClient client,
            HttpRequestData requestMessage)
        {
            var stream = client.GetSwaggerOAuth2Redirect();
            var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            var result = requestMessage.CreateResponse(HttpStatusCode.OK);
            result.WriteString(document);
            result.Headers.Add("Content-Type", "text/html; charset=utf-8");
            return result;
        }

        private static string GetBaseUri(ISwashBuckleClient client, HttpRequestData requestMessage)
        {
            var authority = requestMessage.Url.Authority.TrimEnd('/');
            var scheme = requestMessage.Url.Scheme;
            var host = $"{scheme}://{authority}";

            return host;
        }
    }
}
