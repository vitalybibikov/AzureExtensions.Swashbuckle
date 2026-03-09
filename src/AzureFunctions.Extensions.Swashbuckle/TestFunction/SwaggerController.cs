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
using Microsoft.Extensions.Logging;

namespace TestFunction
{
    public class SwaggerController
    {
        private readonly ISwashBuckleClient swashBuckleClient;
        private readonly ILogger<SwaggerController> logger;

        public SwaggerController(ISwashBuckleClient swashBuckleClient, ILogger<SwaggerController> logger)
        {
            this.swashBuckleClient = swashBuckleClient;
            this.logger = logger;
        }

        [SwaggerIgnore]
        [Function("SwaggerJson")]
        public async Task<IActionResult> SwaggerJson(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Swagger/json")]
            HttpRequest req)
        {
            this.logger.LogWarning("[SWAGGER-DIAG] SwaggerJson: ENTER, Scheme={Scheme}, Host={Host}", req.Scheme, req.Host);
            try
            {
                var result = await this.swashBuckleClient.CreateSwaggerJsonDocumentResult(req);
                var cr = (ContentResult)result;
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerJson: OK, ContentLength={Len}, ContentType={CT}", cr.Content?.Length, cr.ContentType);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "[SWAGGER-DIAG] SwaggerJson: EXCEPTION");
                return new ContentResult { Content = ex.ToString(), ContentType = "text/plain", StatusCode = 500 };
            }
        }

        [SwaggerIgnore]
        [Function("SwaggerYaml")]
        public async Task<IActionResult> SwaggerYaml(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Swagger/yaml")]
            HttpRequest req)
        {
            this.logger.LogWarning("[SWAGGER-DIAG] SwaggerYaml: ENTER");
            try
            {
                var result = await this.swashBuckleClient.CreateSwaggerYamlDocumentResult(req);
                var cr = (ContentResult)result;
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerYaml: OK, ContentLength={Len}", cr.Content?.Length);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "[SWAGGER-DIAG] SwaggerYaml: EXCEPTION");
                return new ContentResult { Content = ex.ToString(), ContentType = "text/plain", StatusCode = 500 };
            }
        }

        [SwaggerIgnore]
        [Function("SwaggerUi")]
        public async Task<IActionResult> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Swagger/ui")]
            HttpRequest req)
        {
            this.logger.LogWarning("[SWAGGER-DIAG] SwaggerUi: ENTER");
            try
            {
                var result = await this.swashBuckleClient.CreateSwaggerUIResult(req, "swagger/json");
                var cr = (ContentResult)result;
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerUi: OK, ContentLength={Len}", cr.Content?.Length);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "[SWAGGER-DIAG] SwaggerUi: EXCEPTION");
                return new ContentResult { Content = ex.ToString(), ContentType = "text/plain", StatusCode = 500 };
            }
        }

        [SwaggerIgnore]
        [Function("SwaggerOAuth2Redirect")]
        public async Task<IActionResult> SwaggerOAuth2Redirect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/oauth2-redirect")]
            HttpRequest req)
        {
            this.logger.LogWarning("[SWAGGER-DIAG] SwaggerOAuth2Redirect: ENTER");
            try
            {
                var result = await this.swashBuckleClient.CreateSwaggerOAuth2RedirectResult();
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerOAuth2Redirect: OK");
                return result;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "[SWAGGER-DIAG] SwaggerOAuth2Redirect: EXCEPTION");
                return new ContentResult { Content = ex.ToString(), ContentType = "text/plain", StatusCode = 500 };
            }
        }

        [SwaggerIgnore]
        [Function("SwaggerValidate")]
        public async Task<IActionResult> Validate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/validate")]
            HttpRequest req)
        {
            this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: ENTER, Scheme={Scheme}, Host={Host}", req.Scheme, req.Host);
            var results = new List<string>();
            var host = $"{req.Scheme}://{req.Host}";

            // Validate JSON generation
            try
            {
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: generating JSON...");
                using var jsonStream = await this.swashBuckleClient.GetSwaggerJsonDocumentAsync(host);
                using var reader = new StreamReader(jsonStream);
                var json = await reader.ReadToEndAsync();
                results.Add(json.Contains("openapi") ? "PASS: JSON contains 'openapi'" : "FAIL: JSON missing 'openapi'");
                results.Add(json.Contains("paths") ? "PASS: JSON contains 'paths'" : "FAIL: JSON missing 'paths'");
                results.Add($"INFO: JSON length = {json.Length}");
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: JSON OK, length={Len}", json.Length);
            }
            catch (Exception ex)
            {
                results.Add($"FAIL: JSON — {ex.GetType().Name}: {ex.Message}");
                this.logger.LogError(ex, "[SWAGGER-DIAG] SwaggerValidate: JSON EXCEPTION");
            }

            // Validate YAML generation
            try
            {
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: generating YAML...");
                using var yamlStream = await this.swashBuckleClient.GetSwaggerYamlDocumentAsync(host);
                using var reader = new StreamReader(yamlStream);
                var yaml = await reader.ReadToEndAsync();
                results.Add(yaml.Contains("openapi:") ? "PASS: YAML contains 'openapi:'" : "FAIL: YAML missing 'openapi:'");
                results.Add(yaml.Contains("paths:") ? "PASS: YAML contains 'paths:'" : "FAIL: YAML missing 'paths:'");
                results.Add($"INFO: YAML length = {yaml.Length}");
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: YAML OK, length={Len}", yaml.Length);
            }
            catch (Exception ex)
            {
                results.Add($"FAIL: YAML — {ex.GetType().Name}: {ex.Message}");
                this.logger.LogError(ex, "[SWAGGER-DIAG] SwaggerValidate: YAML EXCEPTION");
            }

            // Validate Swagger UI generation
            try
            {
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: generating UI...");
                using var uiStream = this.swashBuckleClient.GetSwaggerUi($"{host}/api/swagger/json");
                using var reader = new StreamReader(uiStream);
                var html = reader.ReadToEnd();
                results.Add(html.Contains("<html") ? "PASS: UI contains '<html'" : "FAIL: UI missing '<html'");
                results.Add(html.Contains("swagger") ? "PASS: UI contains 'swagger'" : "FAIL: UI missing 'swagger'");
                results.Add($"INFO: UI length = {html.Length}");
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: UI OK, length={Len}", html.Length);
            }
            catch (Exception ex)
            {
                results.Add($"FAIL: UI — {ex.GetType().Name}: {ex.Message}");
                this.logger.LogError(ex, "[SWAGGER-DIAG] SwaggerValidate: UI EXCEPTION");
            }

            // Validate OAuth2 redirect
            try
            {
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: generating OAuth2...");
                using var oauthStream = this.swashBuckleClient.GetSwaggerOAuth2Redirect();
                using var reader = new StreamReader(oauthStream);
                var html = reader.ReadToEnd();
                results.Add(html.Length > 0 ? "PASS: OAuth2 redirect has content" : "FAIL: OAuth2 redirect empty");
                results.Add($"INFO: OAuth2 length = {html.Length}");
                this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: OAuth2 OK, length={Len}", html.Length);
            }
            catch (Exception ex)
            {
                results.Add($"FAIL: OAuth2 — {ex.GetType().Name}: {ex.Message}");
                this.logger.LogError(ex, "[SWAGGER-DIAG] SwaggerValidate: OAuth2 EXCEPTION");
            }

            var failures = results.Where(r => r.StartsWith("FAIL")).ToList();
            var summary = failures.Count == 0 ? "=== ALL CHECKS PASSED ===" : $"=== {failures.Count} FAILURE(S) ===";
            this.logger.LogWarning("[SWAGGER-DIAG] SwaggerValidate: DONE — {Summary}", summary);

            return new ContentResult
            {
                Content = string.Join("\n", results) + "\n\n" + summary,
                ContentType = "text/plain; charset=utf-8",
                StatusCode = failures.Count == 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError
            };
        }
    }
}
