// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

/// <summary>
/// Tests for <see cref="SwashBuckleClientAspNetCoreExtension"/> — the IActionResult-based
/// extension methods recommended for ConfigureFunctionsWebApplication.
/// </summary>
public class SwashBuckleClientAspNetCoreExtensionTests
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

    private static HttpRequest CreateFakeHttpRequest(string url = "https://localhost:7071/api/Swagger/json")
    {
        var uri = new Uri(url);
        var context = new DefaultHttpContext();
        context.Request.Scheme = uri.Scheme;
        context.Request.Host = new HostString(uri.Host, uri.Port);
        context.Request.Path = uri.AbsolutePath;
        context.Request.Method = "GET";
        return context.Request;
    }

    private static ContentResult AssertContentResult(IActionResult result)
    {
        result.Should().BeOfType<ContentResult>();
        return (ContentResult)result;
    }

    // ─── CreateSwaggerJsonDocumentResult ──────────────────────────────────────

    [Fact]
    public async Task JsonResult_Returns200()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerJsonDocumentResult(CreateFakeHttpRequest()));

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task JsonResult_SetsJsonContentType()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerJsonDocumentResult(CreateFakeHttpRequest()));

        result.ContentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task JsonResult_ContainsValidOpenApiDocument()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerJsonDocumentResult(CreateFakeHttpRequest()));

        result.Content.Should().NotBeNullOrWhiteSpace();
        result.Content.Should().Contain("openapi");
        result.Content.Should().Contain("paths");
        result.Content.Should().Contain("info");
    }

    [Fact]
    public async Task JsonResult_CompletesWithinTimeout()
    {
        var client = CreateClient();
        Func<Task> act = () => client.CreateSwaggerJsonDocumentResult(CreateFakeHttpRequest());
        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(10),
            "JSON result must not hang");
    }

    [Fact]
    public async Task JsonResult_WithCustomDocument_Works()
    {
        var client = CreateClient(opts =>
        {
            opts.Documents = new List<SwaggerDocument>
            {
                new SwaggerDocument { Name = "v2", Title = "V2 API", Version = "2.0" }
            };
        });

        var result = AssertContentResult(
            await client.CreateSwaggerJsonDocumentResult(CreateFakeHttpRequest(), "v2"));

        result.Content.Should().Contain("V2 API");
    }

    // ─── CreateSwaggerYamlDocumentResult ──────────────────────────────────────

    [Fact]
    public async Task YamlResult_Returns200WithYamlContentType()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerYamlDocumentResult(CreateFakeHttpRequest()));

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.ContentType.Should().Contain("application/x-yaml");
    }

    [Fact]
    public async Task YamlResult_ContainsValidOpenApiYaml()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerYamlDocumentResult(CreateFakeHttpRequest()));

        result.Content.Should().NotBeNullOrWhiteSpace();
        result.Content.Should().Contain("openapi:");
        result.Content.Should().Contain("paths:");
    }

    [Fact]
    public async Task YamlResult_CompletesWithinTimeout()
    {
        var client = CreateClient();
        Func<Task> act = () => client.CreateSwaggerYamlDocumentResult(CreateFakeHttpRequest());
        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(10));
    }

    // ─── CreateSwaggerUIResult ────────────────────────────────────────────────

    [Fact]
    public async Task UiResult_Returns200WithHtmlContentType()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerUIResult(
                CreateFakeHttpRequest("https://localhost:7071/api/Swagger/ui"),
                "swagger/json"));

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.ContentType.Should().Contain("text/html");
    }

    [Fact]
    public async Task UiResult_ContainsHtmlWithSwaggerUrl()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerUIResult(
                CreateFakeHttpRequest("https://localhost:7071/api/Swagger/ui"),
                "swagger/json"));

        result.Content.Should().Contain("<html");
        result.Content.Should().Contain("swagger/json");
    }

    [Fact]
    public async Task UiResult_WithRoutePrefix_IncludesPrefixInUrl()
    {
        var client = CreateClient(opts => opts.RoutePrefix = "api");
        var result = AssertContentResult(
            await client.CreateSwaggerUIResult(
                CreateFakeHttpRequest("https://localhost:7071/api/Swagger/ui"),
                "swagger/json"));

        result.Content.Should().Contain("https://localhost:7071/api/swagger/json");
    }

    [Fact]
    public async Task UiResult_HasSubstantialContent()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerUIResult(
                CreateFakeHttpRequest("https://localhost:7071/api/Swagger/ui"),
                "swagger/json"));

        // Swagger UI HTML with inlined CSS+JS should be substantial
        result.Content!.Length.Should().BeGreaterThan(100_000,
            "Swagger UI HTML includes inlined CSS and JS bundles");
    }

    [Fact]
    public async Task UiResult_CompletesWithinTimeout()
    {
        var client = CreateClient();
        Func<Task> act = () => client.CreateSwaggerUIResult(
            CreateFakeHttpRequest("https://localhost:7071/api/Swagger/ui"),
            "swagger/json");
        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(10),
            "Swagger UI result must not hang");
    }

    // ─── CreateSwaggerOAuth2RedirectResult ────────────────────────────────────

    [Fact]
    public async Task OAuth2Result_Returns200WithHtmlContentType()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerOAuth2RedirectResult());

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.ContentType.Should().Contain("text/html");
    }

    [Fact]
    public async Task OAuth2Result_ContainsOAuthContent()
    {
        var client = CreateClient();
        var result = AssertContentResult(
            await client.CreateSwaggerOAuth2RedirectResult());

        result.Content.Should().NotBeNullOrWhiteSpace();
        result.Content.Should().ContainEquivalentOf("oauth");
    }

    [Fact]
    public async Task OAuth2Result_CompletesWithinTimeout()
    {
        var client = CreateClient();
        Func<Task> act = () => client.CreateSwaggerOAuth2RedirectResult();
        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(10));
    }

    // ─── Full pipeline integration ────────────────────────────────────────────

    [Fact]
    public async Task AllEndpoints_ReturnContentResults()
    {
        var client = CreateClient();
        var request = CreateFakeHttpRequest();

        var json = await client.CreateSwaggerJsonDocumentResult(request);
        var yaml = await client.CreateSwaggerYamlDocumentResult(request);
        var ui = await client.CreateSwaggerUIResult(
            CreateFakeHttpRequest("https://localhost:7071/api/Swagger/ui"), "swagger/json");
        var oauth = await client.CreateSwaggerOAuth2RedirectResult();

        foreach (var result in new[] { json, yaml, ui, oauth })
        {
            result.Should().BeOfType<ContentResult>();
            var content = (ContentResult)result;
            content.StatusCode.Should().Be(StatusCodes.Status200OK);
            content.Content.Should().NotBeNullOrWhiteSpace();
            content.ContentType.Should().NotBeNullOrWhiteSpace();
        }
    }
}
