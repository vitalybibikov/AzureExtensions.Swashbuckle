using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureFunctions.Extensions.Swashbuckle
{
    /// <summary>
    /// Extension methods for <see cref="ISwashBuckleClient"/> that return <see cref="IActionResult"/>.
    /// Use these with <c>ConfigureFunctionsWebApplication</c> (ASP.NET Core integration).
    /// </summary>
    public static class SwashBuckleClientAspNetCoreExtension
    {
        public static async Task<IActionResult> CreateSwaggerJsonDocumentResult(
            this ISwashBuckleClient client,
            HttpRequest request,
            string documentName = "v1")
        {
            var host = GetBaseUri(request);

            using var stream = await client.GetSwaggerJsonDocumentAsync(host, documentName);
            using var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();

            return new ContentResult
            {
                Content = document,
                ContentType = "application/json; charset=utf-8",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public static async Task<IActionResult> CreateSwaggerYamlDocumentResult(
            this ISwashBuckleClient client,
            HttpRequest request,
            string documentName = "v1")
        {
            var host = GetBaseUri(request);

            using var stream = await client.GetSwaggerYamlDocumentAsync(host, documentName);
            using var reader = new StreamReader(stream);
            var document = await reader.ReadToEndAsync();

            return new ContentResult
            {
                Content = document,
                ContentType = "application/x-yaml; charset=utf-8",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public static Task<IActionResult> CreateSwaggerUIResult(
            this ISwashBuckleClient client,
            HttpRequest request,
            string documentRoute)
        {
            var routePrefix = string.IsNullOrEmpty(client.RoutePrefix)
                ? string.Empty
                : $"/{client.RoutePrefix}";

            var host = GetBaseUri(request);
            using var stream = client.GetSwaggerUi($"{host}{routePrefix}/{documentRoute}");
            using var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            IActionResult result = new ContentResult
            {
                Content = document,
                ContentType = "text/html; charset=utf-8",
                StatusCode = StatusCodes.Status200OK
            };

            return Task.FromResult(result);
        }

        public static Task<IActionResult> CreateSwaggerOAuth2RedirectResult(
            this ISwashBuckleClient client)
        {
            using var stream = client.GetSwaggerOAuth2Redirect();
            using var reader = new StreamReader(stream);
            var document = reader.ReadToEnd();

            IActionResult result = new ContentResult
            {
                Content = document,
                ContentType = "text/html; charset=utf-8",
                StatusCode = StatusCodes.Status200OK
            };

            return Task.FromResult(result);
        }

        private static string GetBaseUri(HttpRequest request)
        {
            var scheme = request.Scheme;
            var host = request.Host.Value;

            return $"{scheme}://{host}";
        }
    }
}
