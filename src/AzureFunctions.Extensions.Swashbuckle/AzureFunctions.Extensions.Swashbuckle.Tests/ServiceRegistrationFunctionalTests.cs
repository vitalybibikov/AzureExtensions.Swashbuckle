// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

/// <summary>
/// Functional tests that verify AddSwashBuckle registers all required services
/// and that the full DI graph resolves without errors — without any prior
/// AddMvcCore() call. This mimics real Azure Functions startup where only
/// AddSwashBuckle is called.
/// </summary>
public class ServiceRegistrationFunctionalTests
{
    /// <summary>
    /// Builds a ServiceProvider using only AddSwashBuckle (+ AddOptions),
    /// exactly like a real Azure Functions Program.cs would.
    /// No AddMvcCore() — AddSwashBuckle must be self-contained.
    /// </summary>
    private static ServiceProvider BuildMinimalProvider(Action<SwaggerDocOptions>? configure = null)
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

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddSwashBuckle_Standalone_ResolvesISwashBuckleClient()
    {
        using var provider = BuildMinimalProvider();

        var client = provider.GetService<ISwashBuckleClient>();

        client.Should().NotBeNull("ISwashBuckleClient must be resolvable without AddMvcCore");
    }

    [Fact]
    public void AddSwashBuckle_Standalone_ResolvesSwashbuckleConfig()
    {
        using var provider = BuildMinimalProvider();

        var config = provider.GetService<SwashbuckleConfig>();

        config.Should().NotBeNull("SwashbuckleConfig must be resolvable without AddMvcCore");
    }

    [Fact]
    public void AddSwashBuckle_Standalone_ResolvesApiDescriptionProvider()
    {
        using var provider = BuildMinimalProvider();

        var descriptionProvider = provider.GetService<IApiDescriptionGroupCollectionProvider>();

        descriptionProvider.Should().NotBeNull(
            "IApiDescriptionGroupCollectionProvider (FunctionApiDescriptionProvider) must resolve — " +
            "this requires ICompositeMetadataDetailsProvider to be registered");
    }

    [Fact]
    public void AddSwashBuckle_Standalone_ResolvesCompositeMetadataDetailsProvider()
    {
        using var provider = BuildMinimalProvider();

        var composite = provider.GetService<ICompositeMetadataDetailsProvider>();

        composite.Should().NotBeNull(
            "ICompositeMetadataDetailsProvider must be registered by AddSwashBuckle — " +
            "missing registration causes startup crash (0x80008096)");
    }

    [Fact]
    public void AddSwashBuckle_Standalone_ResolvesModelMetadataProvider()
    {
        using var provider = BuildMinimalProvider();

        var metadataProvider = provider.GetService<IModelMetadataProvider>();

        metadataProvider.Should().NotBeNull();
        metadataProvider.Should().BeOfType<EmptyModelMetadataProvider>();
    }

    [Fact]
    public void AddSwashBuckle_Standalone_ResolvesOutputFormatter()
    {
        using var provider = BuildMinimalProvider();

        var formatter = provider.GetService<IOutputFormatter>();

        formatter.Should().NotBeNull();
        formatter.Should().BeOfType<SystemTextJsonOutputFormatter>();
    }

    [Fact]
    public void AddSwashBuckle_Standalone_ResolvesStartupConfig()
    {
        using var provider = BuildMinimalProvider();

        var startupConfig = provider.GetService<SwashBuckleStartupConfig>();

        startupConfig.Should().NotBeNull();
        startupConfig!.Assembly.Should().NotBeNull();
    }

    [Fact]
    public async Task AddSwashBuckle_Standalone_FullPipeline_GeneratesJsonDocument()
    {
        using var provider = BuildMinimalProvider();
        var client = provider.GetRequiredService<ISwashBuckleClient>();

        using var stream = await client.GetSwaggerJsonDocumentAsync("https://localhost");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("openapi", "Full pipeline must produce valid OpenAPI JSON");
        json.Should().Contain("paths", "Full pipeline must produce paths");
    }

    [Fact]
    public async Task AddSwashBuckle_Standalone_FullPipeline_GeneratesYamlDocument()
    {
        using var provider = BuildMinimalProvider();
        var client = provider.GetRequiredService<ISwashBuckleClient>();

        using var stream = await client.GetSwaggerYamlDocumentAsync("https://localhost");
        using var reader = new StreamReader(stream);
        var yaml = await reader.ReadToEndAsync();

        yaml.Should().NotBeNullOrWhiteSpace();
        yaml.Should().Contain("openapi:", "Full pipeline must produce valid OpenAPI YAML");
    }

    [Fact]
    public void AddSwashBuckle_Standalone_FullPipeline_GeneratesSwaggerUi()
    {
        using var provider = BuildMinimalProvider();
        var client = provider.GetRequiredService<ISwashBuckleClient>();

        using var stream = client.GetSwaggerUi("https://localhost/swagger.json");
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        html.Should().Contain("<html", "Swagger UI must return HTML");
    }

    [Fact]
    public void AddSwashBuckle_Standalone_GetRequiredService_DoesNotThrow()
    {
        using var provider = BuildMinimalProvider();

        // This is the critical test: GetRequiredService throws if any
        // dependency in the graph is missing. This is what crashed at runtime.
        var act = () => provider.GetRequiredService<ISwashBuckleClient>();

        act.Should().NotThrow("All dependencies must be resolvable — " +
            "a throw here means AddSwashBuckle is missing a required registration");
    }

    [Fact]
    public void AddSwashBuckle_Standalone_GetRequiredService_ApiDescriptionProvider_DoesNotThrow()
    {
        using var provider = BuildMinimalProvider();

        // FunctionApiDescriptionProvider has the most dependencies (5 ctor params).
        // This is the service that crashed when ICompositeMetadataDetailsProvider was missing.
        var act = () => provider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

        act.Should().NotThrow(
            "FunctionApiDescriptionProvider resolution must not throw — " +
            "missing ICompositeMetadataDetailsProvider caused the 0x80008096 crash");
    }

    [Fact]
    public void AddSwashBuckle_WithExistingMvcCore_DoesNotDuplicateRegistrations()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddMvcCore();
        services.AddSwashBuckle(executingAssembly: Assembly.GetExecutingAssembly());

        using var provider = services.BuildServiceProvider();

        // Should still resolve fine when AddMvcCore was called externally
        var client = provider.GetRequiredService<ISwashBuckleClient>();
        client.Should().NotBeNull();

        var descriptionProvider = provider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
        descriptionProvider.Should().NotBeNull();
    }

    [Fact]
    public async Task AddSwashBuckle_WithCustomDocument_Standalone_FullPipeline()
    {
        using var provider = BuildMinimalProvider(options =>
        {
            options.Documents = new List<SwaggerDocument>
            {
                new SwaggerDocument { Name = "v2", Title = "Test API v2", Version = "2.0" }
            };
        });

        var client = provider.GetRequiredService<ISwashBuckleClient>();
        using var stream = await client.GetSwaggerJsonDocumentAsync("https://localhost", "v2");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        json.Should().Contain("Test API v2");
    }
}
