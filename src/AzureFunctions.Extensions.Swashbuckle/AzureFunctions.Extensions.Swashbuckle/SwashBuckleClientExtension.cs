using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleClientExtension
    {
        public static HttpResponseMessage CreateSwaggerDocumentResponse(this ISwashBuckleClient client,
            HttpRequestMessage requestMessage)
        {
            var stream = client.GetSwaggerDocument();
            var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.RequestMessage = requestMessage;
            response.Content = new StringContent(document, Encoding.UTF8, "application/json");

            return response;
        }

        public static HttpResponseMessage CreateSwaggerUIResponse(this ISwashBuckleClient client,
            HttpRequestMessage requestMessage, string documentRoute)
        {
            var stream =
                client.GetSwaggerUi(
                    $"{requestMessage.RequestUri.Scheme}://{requestMessage.RequestUri.Authority}/{client.RoutePrefix}/{documentRoute}");
            var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.RequestMessage = requestMessage;
            result.Content = new StringContent(document, Encoding.UTF8, "text/html");
            return result;
        }
    }
}
