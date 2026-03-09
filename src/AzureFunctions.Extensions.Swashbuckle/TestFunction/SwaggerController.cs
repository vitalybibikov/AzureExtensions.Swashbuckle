using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [SwaggerIgnore]
        [Function("SwaggerValidate")]
        public async Task<IActionResult> Validate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/validate")]
            HttpRequest req)
        {
            var results = new List<string>();
            var host = $"{req.Scheme}://{req.Host}";

            // Validate JSON generation
            try
            {
                using var jsonStream = await this.swashBuckleClient.GetSwaggerJsonDocumentAsync(host);
                using var reader = new StreamReader(jsonStream);
                var json = await reader.ReadToEndAsync();
                results.Add(json.Contains("openapi") ? "PASS: JSON contains 'openapi'" : "FAIL: JSON missing 'openapi'");
                results.Add(json.Contains("paths") ? "PASS: JSON contains 'paths'" : "FAIL: JSON missing 'paths'");
                results.Add($"INFO: JSON length = {json.Length}");
            }
            catch (Exception ex)
            {
                results.Add($"FAIL: JSON — {ex.GetType().Name}: {ex.Message}");
            }

            // Validate YAML generation
            try
            {
                using var yamlStream = await this.swashBuckleClient.GetSwaggerYamlDocumentAsync(host);
                using var reader = new StreamReader(yamlStream);
                var yaml = await reader.ReadToEndAsync();
                results.Add(yaml.Contains("openapi:") ? "PASS: YAML contains 'openapi:'" : "FAIL: YAML missing 'openapi:'");
                results.Add(yaml.Contains("paths:") ? "PASS: YAML contains 'paths:'" : "FAIL: YAML missing 'paths:'");
                results.Add($"INFO: YAML length = {yaml.Length}");
            }
            catch (Exception ex)
            {
                results.Add($"FAIL: YAML — {ex.GetType().Name}: {ex.Message}");
            }

            // Validate Swagger UI generation
            try
            {
                using var uiStream = this.swashBuckleClient.GetSwaggerUi($"{host}/api/swagger/json");
                using var reader = new StreamReader(uiStream);
                var html = reader.ReadToEnd();
                results.Add(html.Contains("<html") ? "PASS: UI contains '<html'" : "FAIL: UI missing '<html'");
                results.Add(html.Contains("swagger") ? "PASS: UI contains 'swagger'" : "FAIL: UI missing 'swagger'");
                results.Add($"INFO: UI length = {html.Length}");
            }
            catch (Exception ex)
            {
                results.Add($"FAIL: UI — {ex.GetType().Name}: {ex.Message}");
            }

            // Validate OAuth2 redirect
            try
            {
                using var oauthStream = this.swashBuckleClient.GetSwaggerOAuth2Redirect();
                using var reader = new StreamReader(oauthStream);
                var html = reader.ReadToEnd();
                results.Add(html.Length > 0 ? "PASS: OAuth2 redirect has content" : "FAIL: OAuth2 redirect empty");
                results.Add($"INFO: OAuth2 length = {html.Length}");
            }
            catch (Exception ex)
            {
                results.Add($"FAIL: OAuth2 — {ex.GetType().Name}: {ex.Message}");
            }

            var allPassed = results.Where(r => r.StartsWith("FAIL")).ToList();

            return new ContentResult
            {
                Content = string.Join("\n", results) + "\n\n" +
                          (allPassed.Count == 0 ? "=== ALL CHECKS PASSED ===" : $"=== {allPassed.Count} FAILURE(S) ==="),
                ContentType = "text/plain; charset=utf-8",
                StatusCode = allPassed.Count == 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError
            };
        }
    }
}
