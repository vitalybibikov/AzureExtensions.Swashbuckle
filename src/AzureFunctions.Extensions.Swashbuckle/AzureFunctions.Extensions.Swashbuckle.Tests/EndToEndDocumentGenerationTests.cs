// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class EndToEndDocumentGenerationTests
{
    private static ISwashBuckleClient CreateClient(Action<SwaggerDocOptions>? configureOptions = null)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddMvcCore();
        services.AddSwashBuckle(
            configureDocOptionsAction: options =>
            {
                options.ConfigureSwaggerGen = swaggerGen =>
                {
                    swaggerGen.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                };
                configureOptions?.Invoke(options);
            },
            executingAssembly: Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ISwashBuckleClient>();
    }

    private static async Task<JsonDocument> GetJsonDocument(
        ISwashBuckleClient client, string documentName = "v1")
    {
        using var stream = await client.GetSwaggerJsonDocumentAsync("https://localhost", documentName);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        return JsonDocument.Parse(json);
    }

    private static JsonElement? FindPathContaining(JsonElement paths, string substring)
    {
        foreach (var path in paths.EnumerateObject())
        {
            if (path.Name.Contains(substring))
            {
                return path.Value;
            }
        }

        return null;
    }

    private static bool HasPath(JsonElement paths, string exactPath)
    {
        foreach (var path in paths.EnumerateObject())
        {
            if (path.Name == exactPath)
            {
                return true;
            }
        }

        return false;
    }

    [Fact]
    public async Task GeneratedDocument_HasCorrectPathsForTestFunctions()
    {
        var client = CreateClient();

        using var doc = await GetJsonDocument(client);
        var root = doc.RootElement;

        root.TryGetProperty("paths", out var paths).Should().BeTrue("Document should have paths");

        var pathKeys = new List<string>();
        foreach (var path in paths.EnumerateObject())
        {
            pathKeys.Add(path.Name);
        }

        pathKeys.Should().Contain(p => p.Contains("items"),
            "Should contain an 'items' path from test functions");
    }

    [Fact]
    public async Task GeneratedDocument_GetItemsEndpointExists()
    {
        var client = CreateClient();

        using var doc = await GetJsonDocument(client);
        var paths = doc.RootElement.GetProperty("paths");

        var itemsPath = FindPathContaining(paths, "/items");
        itemsPath.Should().NotBeNull("GET /items endpoint should exist");

        itemsPath!.Value.TryGetProperty("get", out _).Should().BeTrue(
            "items path should have a GET operation");
    }

    [Fact]
    public async Task GeneratedDocument_PostItemsEndpointExists()
    {
        var client = CreateClient();

        using var doc = await GetJsonDocument(client);
        var paths = doc.RootElement.GetProperty("paths");

        var itemsPath = FindPathContaining(paths, "/items");
        itemsPath.Should().NotBeNull();

        itemsPath!.Value.TryGetProperty("post", out _).Should().BeTrue(
            "items path should have a POST operation");
    }

    [Fact]
    public async Task GeneratedDocument_PathParameterPresent()
    {
        var client = CreateClient();

        using var doc = await GetJsonDocument(client);
        var paths = doc.RootElement.GetProperty("paths");

        var idPath = FindPathContaining(paths, "{id}");
        idPath.Should().NotBeNull("A path with {id} parameter should exist");
    }

    [Fact]
    public async Task GeneratedDocument_SwaggerIgnoreEndpointIsNotPresent()
    {
        var client = CreateClient();

        using var doc = await GetJsonDocument(client);
        var paths = doc.RootElement.GetProperty("paths");

        HasPath(paths, "/hidden").Should().BeFalse("SwaggerIgnore endpoint should not appear in the document");
    }

    [Fact]
    public async Task GeneratedDocument_FunctionContextIsNotAParameter()
    {
        var client = CreateClient();

        using var doc = await GetJsonDocument(client);
        var paths = doc.RootElement.GetProperty("paths");

        var contextPath = FindPathContaining(paths, "context");
        contextPath.Should().NotBeNull();

        var getOp = contextPath!.Value.GetProperty("get");
        if (getOp.TryGetProperty("parameters", out var parameters))
        {
            foreach (var param in parameters.EnumerateArray())
            {
                var name = param.GetProperty("name").GetString();
                name.Should().NotBe("context",
                    "FunctionContext should not appear as a parameter");
            }
        }
    }

    [Fact]
    public async Task GeneratedDocument_ResponseTypesFromProducesResponseType()
    {
        var client = CreateClient();

        using var doc = await GetJsonDocument(client);
        var paths = doc.RootElement.GetProperty("paths");

        var responsesPath = FindPathContaining(paths, "responses");
        responsesPath.Should().NotBeNull();

        var getOp = responsesPath!.Value.GetProperty("get");
        getOp.TryGetProperty("responses", out var responses).Should().BeTrue();

        var responseKeys = new List<string>();
        foreach (var resp in responses.EnumerateObject())
        {
            responseKeys.Add(resp.Name);
        }

        responseKeys.Should().Contain("200");
        responseKeys.Should().Contain("404");
    }

    [Fact]
    public async Task GeneratedDocument_MultipleDocuments_EachResolvable()
    {
        var client = CreateClient(options =>
        {
            options.Documents = new List<SwaggerDocument>
            {
                new SwaggerDocument { Name = "v1", Title = "API v1", Version = "1.0" },
                new SwaggerDocument { Name = "v2", Title = "API v2", Version = "2.0" }
            };
        });

        using var doc1 = await GetJsonDocument(client, "v1");
        var info1 = doc1.RootElement.GetProperty("info");
        info1.GetProperty("title").GetString().Should().Be("API v1");

        using var doc2 = await GetJsonDocument(client, "v2");
        var info2 = doc2.RootElement.GetProperty("info");
        info2.GetProperty("title").GetString().Should().Be("API v2");
    }

    [Fact]
    public async Task GeneratedDocument_YamlGeneration_Works()
    {
        var client = CreateClient();

        using var stream = await client.GetSwaggerYamlDocumentAsync("https://localhost");
        using var reader = new StreamReader(stream);
        var yaml = await reader.ReadToEndAsync();

        yaml.Should().NotBeNullOrWhiteSpace();
        yaml.Should().Contain("openapi:");
        yaml.Should().Contain("paths:");
    }

    [Fact]
    public async Task GeneratedDocument_OpenApi20_ProducesSwagger2Format()
    {
        var client = CreateClient(options =>
        {
            options.SpecVersion = OpenApiSpecVersion.OpenApi2_0;
        });

        using var stream = await client.GetSwaggerJsonDocumentAsync("https://localhost");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        json.Should().Contain("swagger", "OpenAPI 2.0 should contain 'swagger' key");
    }

    [Fact]
    public async Task GeneratedDocument_OpenApi30_ProducesOpenApi3Format()
    {
        var client = CreateClient(options =>
        {
            options.SpecVersion = OpenApiSpecVersion.OpenApi3_0;
        });

        using var stream = await client.GetSwaggerJsonDocumentAsync("https://localhost");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        json.Should().Contain("openapi", "OpenAPI 3.0 should contain 'openapi' key");
    }

    [Fact]
    public async Task GeneratedDocument_HasInfoSection()
    {
        var client = CreateClient(options =>
        {
            options.Documents = new List<SwaggerDocument>
            {
                new SwaggerDocument
                {
                    Name = "v1",
                    Title = "My Test API",
                    Version = "1.0.0",
                    Description = "Test API description"
                }
            };
        });

        using var doc = await GetJsonDocument(client);
        var info = doc.RootElement.GetProperty("info");

        info.GetProperty("title").GetString().Should().Be("My Test API");
        info.GetProperty("version").GetString().Should().Be("1.0.0");
        info.GetProperty("description").GetString().Should().Be("Test API description");
    }

    [Fact]
    public async Task GeneratedDocument_PostItemsEndpoint_HasRequestBodyOrBodyParam()
    {
        var client = CreateClient();

        using var doc = await GetJsonDocument(client);
        var root = doc.RootElement;
        var paths = root.GetProperty("paths");

        var itemsPath = FindPathContaining(paths, "/items");
        itemsPath.Should().NotBeNull();

        var postOp = itemsPath!.Value.GetProperty("post");

        // In OpenAPI 3.0, request body is in "requestBody"
        // In OpenAPI 2.0, body params are in parameters with "in": "body"
        var hasRequestBody = postOp.TryGetProperty("requestBody", out _);
        var hasBodyParam = false;

        if (!hasRequestBody && postOp.TryGetProperty("parameters", out var parameters))
        {
            foreach (var param in parameters.EnumerateArray())
            {
                if (param.TryGetProperty("in", out var inProp) && inProp.GetString() == "body")
                {
                    hasBodyParam = true;
                    break;
                }
            }
        }

        (hasRequestBody || hasBodyParam).Should().BeTrue(
            "POST items should have a request body or body parameter");
    }

    [Fact]
    public async Task GeneratedDocument_AuthLevelFunction_HasCodeParameter()
    {
        var client = CreateClient(options =>
        {
            options.AddCodeParameter = true;
        });

        using var doc = await GetJsonDocument(client);
        var paths = doc.RootElement.GetProperty("paths");

        var securePath = FindPathContaining(paths, "secure");
        securePath.Should().NotBeNull();

        var getOp = securePath!.Value.GetProperty("get");
        getOp.TryGetProperty("parameters", out var parameters).Should().BeTrue();

        var paramNames = new List<string>();
        foreach (var param in parameters.EnumerateArray())
        {
            paramNames.Add(param.GetProperty("name").GetString()!);
        }

        paramNames.Should().Contain("code",
            "Function auth level should add 'code' query parameter when AddCodeParameter=true");
    }
}
