// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class SwashBuckleStartupExtensionTests
{
    [Fact]
    public void AddSwashBuckle_RegistersISwashBuckleClient()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddSwashBuckle(executingAssembly: Assembly.GetExecutingAssembly());

        services.Should().Contain(sd =>
            sd.ServiceType == typeof(ISwashBuckleClient) &&
            sd.ImplementationType == typeof(SwashBuckleClient) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddSwashBuckle_RegistersSwashbuckleConfig()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddSwashBuckle(executingAssembly: Assembly.GetExecutingAssembly());

        services.Should().Contain(sd =>
            sd.ServiceType == typeof(SwashbuckleConfig) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddSwashBuckle_WithOptions_ConfiguresSwaggerDocOptions()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddSwashBuckle(
            configureDocOptionsAction: options =>
            {
                options.Title = "My Test API";
                options.RoutePrefix = "swagger";
            },
            executingAssembly: Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetService<IOptions<SwaggerDocOptions>>();
        optionsMonitor.Should().NotBeNull();
        optionsMonitor!.Value.Title.Should().Be("My Test API");
        optionsMonitor.Value.RoutePrefix.Should().Be("swagger");
    }

    [Fact]
    public void AddSwashBuckle_ReturnsServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        var result = services.AddSwashBuckle(executingAssembly: Assembly.GetExecutingAssembly());

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddSwashBuckle_WithoutOptions_UsesDefaultConfiguration()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddSwashBuckle(executingAssembly: Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetService<IOptions<SwaggerDocOptions>>();
        optionsMonitor.Should().NotBeNull();
        optionsMonitor!.Value.AddCodeParameter.Should().BeTrue();
        optionsMonitor.Value.PrependOperationWithRoutePrefix.Should().BeTrue();
    }

    [Fact]
    public void AddSwashBuckle_RegistersModelMetadataProvider()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddSwashBuckle(executingAssembly: Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        var metadataProvider = provider.GetService<IModelMetadataProvider>();
        metadataProvider.Should().NotBeNull();
        metadataProvider.Should().BeOfType<EmptyModelMetadataProvider>();
    }

    [Fact]
    public void AddSwashBuckle_RegistersOutputFormatter()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddSwashBuckle(executingAssembly: Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        var formatter = provider.GetService<IOutputFormatter>();
        formatter.Should().NotBeNull();
        formatter.Should().BeOfType<SystemTextJsonOutputFormatter>();
    }

    [Fact]
    public void AddSwashBuckle_RegistersApiDescriptionProvider()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddSwashBuckle(executingAssembly: Assembly.GetExecutingAssembly());

        services.Should().Contain(sd =>
            sd.ServiceType == typeof(IApiDescriptionGroupCollectionProvider) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddSwashBuckle_RegistersStartupConfig()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        var assembly = Assembly.GetExecutingAssembly();

        services.AddSwashBuckle(executingAssembly: assembly);

        var provider = services.BuildServiceProvider();
        var startupConfig = provider.GetService<SwashBuckleStartupConfig>();
        startupConfig.Should().NotBeNull();
        startupConfig!.Assembly.Should().BeSameAs(assembly);
    }
}
