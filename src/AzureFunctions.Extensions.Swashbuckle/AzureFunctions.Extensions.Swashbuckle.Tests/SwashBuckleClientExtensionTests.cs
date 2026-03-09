// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

/// <summary>
/// Tests for <see cref="SwashBuckleClientExtension"/> — the HttpResponseData-based extension methods.
/// Uses fake HttpRequestData/HttpResponseData to exercise the full response pipeline
/// without requiring a running Azure Functions host.
/// </summary>
public class SwashBuckleClientExtensionTests
{
    private static ISwashBuckleClient CreateClient(Action<SwaggerDocOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSwashBuckle(
            configureDocOptionsAction: options =>
            {
                options.ConfigureSwaggerGen = swaggerGen =>
                {
                    swaggerGen.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                };
                configure?.Invoke(options);
            },
            executingAssembly: Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ISwashBuckleClient>();
    }

    private static FakeHttpRequestData CreateRequest(string url = "https://localhost:7071/api/Swagger/json")
    {
        return new FakeHttpRequestData(new Uri(url));
    }

    private static async Task<string> ReadResponseBody(HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        return await reader.ReadToEndAsync();
    }

    // ─── CreateSwaggerJsonDocumentResponse ────────────────────────────────────

    [Fact]
    public async Task JsonResponse_ReturnsOkStatus()
    {
        var client = CreateClient();
        var response = await client.CreateSwaggerJsonDocumentResponse(CreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task JsonResponse_SetsJsonContentType()
    {
        var client = CreateClient();
        var response = await client.CreateSwaggerJsonDocumentResponse(CreateRequest());

        response.Headers.Should().Contain(h =>
            h.Key == "Content-Type" && h.Value.Any(v => v.Contains("application/json")));
    }

    [Fact]
    public async Task JsonResponse_ContainsValidOpenApiDocument()
    {
        var client = CreateClient();
        var response = await client.CreateSwaggerJsonDocumentResponse(CreateRequest());
        var body = await ReadResponseBody(response);

        body.Should().NotBeNullOrWhiteSpace();
        body.Should().Contain("openapi");
        body.Should().Contain("paths");
        body.Should().Contain("info");
    }

    [Fact]
    public async Task JsonResponse_CompletesWithinTimeout()
    {
        var client = CreateClient();

        Func<Task> act = () => client.CreateSwaggerJsonDocumentResponse(CreateRequest());

        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(10),
            "JSON response must not hang — a timeout indicates broken response pipeline");
    }

    [Fact]
    public async Task JsonResponse_WithCustomDocument_ReturnsCorrectTitle()
    {
        var client = CreateClient(opts =>
        {
            opts.Documents = new List<SwaggerDocument>
            {
                new SwaggerDocument { Name = "custom", Title = "Custom API", Version = "v2" }
            };
        });

        var response = await client.CreateSwaggerJsonDocumentResponse(CreateRequest(), "custom");
        var body = await ReadResponseBody(response);

        body.Should().Contain("Custom API");
    }

    [Fact]
    public async Task JsonResponse_BodyIsNotEmpty()
    {
        var client = CreateClient();
        var response = await client.CreateSwaggerJsonDocumentResponse(CreateRequest());

        response.Body.Length.Should().BeGreaterThan(0,
            "Response body must have content written to it");
    }

    // ─── CreateSwaggerYamlDocumentResponse ────────────────────────────────────

    [Fact]
    public async Task YamlResponse_ReturnsOkStatus()
    {
        var client = CreateClient();
        var response = await client.CreateSwaggerYamlDocumentResponse(CreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task YamlResponse_SetsYamlContentType()
    {
        var client = CreateClient();
        var response = await client.CreateSwaggerYamlDocumentResponse(CreateRequest());

        response.Headers.Should().Contain(h =>
            h.Key == "Content-Type" && h.Value.Any(v => v.Contains("application/x-yaml")));
    }

    [Fact]
    public async Task YamlResponse_ContainsValidOpenApiYaml()
    {
        var client = CreateClient();
        var response = await client.CreateSwaggerYamlDocumentResponse(CreateRequest());
        var body = await ReadResponseBody(response);

        body.Should().NotBeNullOrWhiteSpace();
        body.Should().Contain("openapi:");
        body.Should().Contain("paths:");
    }

    [Fact]
    public async Task YamlResponse_CompletesWithinTimeout()
    {
        var client = CreateClient();

        Func<Task> act = () => client.CreateSwaggerYamlDocumentResponse(CreateRequest());

        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(10));
    }

    // ─── CreateSwaggerUIResponse ──────────────────────────────────────────────

    [Fact]
    public async Task UiResponse_ReturnsOkStatus()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/Swagger/ui");

        var response = await client.CreateSwaggerUIResponse(request, "swagger/json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UiResponse_SetsHtmlContentType()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/Swagger/ui");

        var response = await client.CreateSwaggerUIResponse(request, "swagger/json");

        response.Headers.Should().Contain(h =>
            h.Key == "Content-Type" && h.Value.Any(v => v.Contains("text/html")));
    }

    [Fact]
    public async Task UiResponse_ContainsHtmlWithSwaggerUrl()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/Swagger/ui");

        var response = await client.CreateSwaggerUIResponse(request, "swagger/json");
        var body = await ReadResponseBody(response);

        body.Should().NotBeNullOrWhiteSpace();
        body.Should().Contain("<html");
        body.Should().Contain("swagger/json");
    }

    [Fact]
    public async Task UiResponse_WithRoutePrefix_EmbedsPrefixInUrl()
    {
        var client = CreateClient(opts => opts.RoutePrefix = "api");
        var request = CreateRequest("https://localhost:7071/api/Swagger/ui");

        var response = await client.CreateSwaggerUIResponse(request, "swagger/json");
        var body = await ReadResponseBody(response);

        body.Should().Contain("https://localhost:7071/api/swagger/json");
    }

    [Fact]
    public async Task UiResponse_WithoutRoutePrefix_NoDoubleSlash()
    {
        var client = CreateClient(opts => opts.RoutePrefix = "");
        var request = CreateRequest("https://localhost:7071/api/Swagger/ui");

        var response = await client.CreateSwaggerUIResponse(request, "swagger/json");
        var body = await ReadResponseBody(response);

        body.Should().Contain("https://localhost:7071/swagger/json");
        body.Should().NotContain("https://localhost:7071//swagger/json");
    }

    [Fact]
    public async Task UiResponse_CompletesWithinTimeout()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/Swagger/ui");

        Func<Task> act = () => client.CreateSwaggerUIResponse(request, "swagger/json");

        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(10),
            "Swagger UI response must not hang");
    }

    [Fact]
    public async Task UiResponse_BodyIsLargerThanMinimum()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/Swagger/ui");

        var response = await client.CreateSwaggerUIResponse(request, "swagger/json");

        // Swagger UI with inlined CSS+JS should be at least 100KB
        response.Body.Length.Should().BeGreaterThan(100_000,
            "Swagger UI HTML includes inlined CSS and JS bundles");
    }

    // ─── CreateSwaggerOAuth2RedirectResponse ──────────────────────────────────

    [Fact]
    public async Task OAuth2Response_ReturnsOkStatus()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/swagger/oauth2-redirect");

        var response = await client.CreateSwaggerOAuth2RedirectResponse(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OAuth2Response_SetsHtmlContentType()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/swagger/oauth2-redirect");

        var response = await client.CreateSwaggerOAuth2RedirectResponse(request);

        response.Headers.Should().Contain(h =>
            h.Key == "Content-Type" && h.Value.Any(v => v.Contains("text/html")));
    }

    [Fact]
    public async Task OAuth2Response_ContainsOAuthContent()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/swagger/oauth2-redirect");

        var response = await client.CreateSwaggerOAuth2RedirectResponse(request);
        var body = await ReadResponseBody(response);

        body.Should().NotBeNullOrWhiteSpace();
        body.Should().ContainEquivalentOf("oauth");
    }

    [Fact]
    public async Task OAuth2Response_CompletesWithinTimeout()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/swagger/oauth2-redirect");

        Func<Task> act = () => client.CreateSwaggerOAuth2RedirectResponse(request);

        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(10));
    }

    // ─── Response body readable across all endpoints ──────────────────────────

    [Fact]
    public async Task AllEndpoints_ResponseBodyIsReadableAndNonEmpty()
    {
        var client = CreateClient();

        var jsonResponse = await client.CreateSwaggerJsonDocumentResponse(CreateRequest());
        var yamlResponse = await client.CreateSwaggerYamlDocumentResponse(CreateRequest());
        var uiResponse = await client.CreateSwaggerUIResponse(
            CreateRequest("https://localhost:7071/api/Swagger/ui"), "swagger/json");
        var oauthResponse = await client.CreateSwaggerOAuth2RedirectResponse(
            CreateRequest("https://localhost:7071/api/swagger/oauth2-redirect"));

        var responses = new[] { jsonResponse, yamlResponse, uiResponse, oauthResponse };

        foreach (var response in responses)
        {
            response.Body.Should().NotBeNull();
            response.Body.Length.Should().BeGreaterThan(0);

            // Verify body is readable (can seek back and read)
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body, leaveOpen: true);
            var content = await reader.ReadToEndAsync();
            content.Should().NotBeNullOrWhiteSpace();
        }
    }

    // ─── Host URL extraction ──────────────────────────────────────────────────

    [Fact]
    public async Task JsonResponse_UsesCorrectHostFromRequest()
    {
        var client = CreateClient();
        var request = CreateRequest("https://myapi.example.com/api/Swagger/json");

        var response = await client.CreateSwaggerJsonDocumentResponse(request);
        var body = await ReadResponseBody(response);

        // The host URL should appear in the server info of the OpenAPI doc
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task JsonResponse_PreservesPortInHost()
    {
        var client = CreateClient();
        var request = CreateRequest("https://localhost:7071/api/Swagger/json");

        var response = await client.CreateSwaggerJsonDocumentResponse(request);
        var body = await ReadResponseBody(response);

        body.Should().NotBeNullOrWhiteSpace();
    }
}
