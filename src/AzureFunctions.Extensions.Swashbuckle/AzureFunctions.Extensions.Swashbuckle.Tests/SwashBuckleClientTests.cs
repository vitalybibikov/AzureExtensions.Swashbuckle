// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class SwashBuckleClientTests
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

    [Fact]
    public void GetSwaggerUi_ReturnsReadableStreamWithHtmlContent()
    {
        var client = CreateClient();

        using var stream = client.GetSwaggerUi("https://localhost/swagger.json");

        stream.Should().NotBeNull();
        stream.CanRead.Should().BeTrue();
        stream.Length.Should().BeGreaterThan(0);

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        content.Should().Contain("<html", "Swagger UI should return HTML content");
    }

    [Fact]
    public void GetSwaggerOAuth2Redirect_ReturnsReadableStream()
    {
        var client = CreateClient();

        using var stream = client.GetSwaggerOAuth2Redirect();

        stream.Should().NotBeNull();
        stream.CanRead.Should().BeTrue();
        stream.Length.Should().BeGreaterThan(0);

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        content.Should().NotBeNullOrWhiteSpace();
        content.Should().ContainEquivalentOf("oauth", "OAuth2 redirect page should contain OAuth references");
    }

    [Fact]
    public void RoutePrefix_DelegatesToConfig()
    {
        var client = CreateClient(options =>
        {
            options.RoutePrefix = "my-swagger";
        });

        client.RoutePrefix.Should().Be("my-swagger");
    }

    [Fact]
    public void RoutePrefix_DefaultsToEmptyString()
    {
        var client = CreateClient();

        client.RoutePrefix.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSwaggerJsonDocumentAsync_ReturnsValidStream()
    {
        var client = CreateClient();

        using var stream = await client.GetSwaggerJsonDocumentAsync("https://localhost");

        stream.Should().NotBeNull();
        stream.CanRead.Should().BeTrue();
        stream.Length.Should().BeGreaterThan(0);

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        content.Should().Contain("openapi", "JSON document should contain OpenAPI content");
    }

    [Fact]
    public async Task GetSwaggerYamlDocumentAsync_ReturnsValidStream()
    {
        var client = CreateClient();

        using var stream = await client.GetSwaggerYamlDocumentAsync("https://localhost");

        stream.Should().NotBeNull();
        stream.CanRead.Should().BeTrue();
        stream.Length.Should().BeGreaterThan(0);

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        content.Should().Contain("openapi", "YAML document should contain OpenAPI content");
    }

    [Fact]
    public async Task GetSwaggerJsonDocumentAsync_WithCustomDocumentName_Works()
    {
        var client = CreateClient(options =>
        {
            options.Documents = new List<SwaggerDocument>
            {
                new SwaggerDocument { Name = "custom", Title = "Custom API", Version = "v2" }
            };
        });

        using var stream = await client.GetSwaggerJsonDocumentAsync("https://localhost", "custom");

        stream.Should().NotBeNull();
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetSwaggerUi_ContainsSwaggerUrl()
    {
        var client = CreateClient();
        var swaggerUrl = "https://myapi.example.com/swagger.json";

        using var stream = client.GetSwaggerUi(swaggerUrl);
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        content.Should().Contain(swaggerUrl, "The Swagger UI HTML should contain the provided swagger URL");
    }
}
