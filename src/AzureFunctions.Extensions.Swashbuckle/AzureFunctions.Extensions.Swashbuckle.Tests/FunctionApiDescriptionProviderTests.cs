// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

// Test model used by RequestBodyType attribute
public class TestItemModel
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

// Test function classes that FunctionApiDescriptionProvider will scan.
// These must be public non-nested to be discoverable via Assembly.GetTypes().
public class TestFunctionEndpoints
{
    [Function("GetItems")]
    public Task<IActionResult> GetItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "items")] HttpRequestData req)
        => Task.FromResult<IActionResult>(null!);

    [Function("CreateItem")]
    public Task<IActionResult> CreateItem(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "items")]
        [RequestBodyType(typeof(TestItemModel), "item")]
        HttpRequestData req)
        => Task.FromResult<IActionResult>(null!);

    [Function("GetItemById")]
    public Task<IActionResult> GetItemById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "items/{id}")] HttpRequestData req,
        string id)
        => Task.FromResult<IActionResult>(null!);

    [Function("WithFunctionContext")]
    public Task<IActionResult> WithFunctionContext(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "context")] HttpRequestData req,
        FunctionContext context)
        => Task.FromResult<IActionResult>(null!);

    [SwaggerIgnore]
    [Function("HiddenEndpoint")]
    public Task<IActionResult> HiddenEndpoint(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hidden")] HttpRequestData req)
        => Task.FromResult<IActionResult>(null!);

    [Function("MultiVerb")]
    public Task<IActionResult> MultiVerb(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "multi")] HttpRequestData req)
        => Task.FromResult<IActionResult>(null!);

    [Function("WithResponseTypes")]
    [ProducesResponseType(typeof(TestItemModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> WithResponseTypes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "responses")] HttpRequestData req)
        => Task.FromResult<IActionResult>(null!);

    [Function("WithRouteConstraint")]
    public Task<IActionResult> WithRouteConstraint(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id:int}")] HttpRequestData req,
        int id)
        => Task.FromResult<IActionResult>(null!);

    [Function("WithOptionalParam")]
    public Task<IActionResult> WithOptionalParam(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "optional/{name?}")] HttpRequestData req,
        string name)
        => Task.FromResult<IActionResult>(null!);

    [Function("AuthLevelFunction")]
    public Task<IActionResult> AuthLevelFunction(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "secure")] HttpRequestData req)
        => Task.FromResult<IActionResult>(null!);
}

public class TestControllerEndpoints
{
    [Function("ControllerAction")]
    public Task<IActionResult> ControllerAction(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ctrl")] HttpRequestData req)
        => Task.FromResult<IActionResult>(null!);
}

public class TestItemsController
{
    [Function("ControllerStripTest")]
    public Task<IActionResult> ControllerStripTest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "strip")] HttpRequestData req)
        => Task.FromResult<IActionResult>(null!);
}

public class FunctionApiDescriptionProviderTests
{
    private static FunctionApiDescriptionProvider CreateProvider(
        SwaggerDocOptions? options = null,
        Assembly? assembly = null)
    {
        // Use MvcCoreServiceCollectionExtensions to get a proper ICompositeMetadataDetailsProvider
        var services = new ServiceCollection();
        services.AddMvcCore();
        var tempProvider = services.BuildServiceProvider();
        var compositeProvider = tempProvider.GetRequiredService<ICompositeMetadataDetailsProvider>();

        var docOptions = options ?? new SwaggerDocOptions();
        var optionsWrapper = Options.Create(docOptions);
        var startupConfig = new SwashBuckleStartupConfig
        {
            Assembly = assembly ?? Assembly.GetExecutingAssembly()
        };
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
        var formatter = new SystemTextJsonOutputFormatter(jsonOptions);

        return new FunctionApiDescriptionProvider(
            optionsWrapper,
            startupConfig,
            modelMetadataProvider,
            compositeProvider,
            formatter);
    }

    private static Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription? FindDescription(
        FunctionApiDescriptionProvider provider, string route, string httpMethod)
    {
        return provider.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .FirstOrDefault(d =>
                d.RelativePath == route &&
                d.HttpMethod!.Equals(httpMethod, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription> FindAllDescriptions(
        FunctionApiDescriptionProvider provider, string route)
    {
        return provider.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .Where(d => d.RelativePath == route);
    }

    [Fact]
    public void DiscoversFunctionsWithHttpTrigger()
    {
        var provider = CreateProvider();

        var allDescriptions = provider.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .ToList();

        // Should find our test endpoints (at minimum)
        allDescriptions.Should().NotBeEmpty();

        // GetItems should be discoverable
        var getItems = FindDescription(provider, "items", "GET");
        getItems.Should().NotBeNull("GetItems function with route 'items' should be discovered");
    }

    [Fact]
    public void ExcludesSwaggerIgnoreMethods()
    {
        var provider = CreateProvider();

        var hidden = FindDescription(provider, "hidden", "GET");
        hidden.Should().BeNull("Methods with [SwaggerIgnore] should be excluded");
    }

    [Fact]
    public void RoutePathParameters_DetectedCorrectly()
    {
        var provider = CreateProvider();

        var getById = FindDescription(provider, "items/{id}", "GET");
        getById.Should().NotBeNull();

        var idParam = getById!.ParameterDescriptions
            .FirstOrDefault(p => p.Name == "id");
        idParam.Should().NotBeNull("'id' should be a parameter");
        idParam!.Source.Should().Be(BindingSource.Path, "route parameter should have Path binding source");
    }

    [Fact]
    public void FunctionContextParameter_IsExcluded()
    {
        var provider = CreateProvider();

        var withContext = FindDescription(provider, "context", "GET");
        withContext.Should().NotBeNull();

        var contextParam = withContext!.ParameterDescriptions
            .FirstOrDefault(p => p.Name == "context");
        contextParam.Should().BeNull("FunctionContext parameter should be excluded from API description");
    }

    [Fact]
    public void HttpRequestDataWithoutRequestBodyType_IsExcluded()
    {
        var provider = CreateProvider();

        var getItems = FindDescription(provider, "items", "GET");
        getItems.Should().NotBeNull();

        var reqParam = getItems!.ParameterDescriptions
            .FirstOrDefault(p => p.Name == "req");
        reqParam.Should().BeNull(
            "HttpRequestData without [RequestBodyType] should be excluded from parameters");
    }

    [Fact]
    public void RequestBodyType_OverridesParameterType()
    {
        var provider = CreateProvider();

        var createItem = FindDescription(provider, "items", "POST");
        createItem.Should().NotBeNull();

        var reqParam = createItem!.ParameterDescriptions
            .FirstOrDefault(p => p.Name == "req");
        reqParam.Should().NotBeNull(
            "HttpRequestData WITH [RequestBodyType] should be included as a parameter");
        reqParam!.Type.Should().Be(typeof(TestItemModel),
            "RequestBodyType should override the parameter type");
    }

    [Fact]
    public void PrependOperationWithRoutePrefix_True_PrependsPrefix()
    {
        var options = new SwaggerDocOptions
        {
            PrependOperationWithRoutePrefix = true,
            RoutePrefix = "api"
        };
        var provider = CreateProvider(options);

        var getItems = FindDescription(provider, "api/items", "GET");
        getItems.Should().NotBeNull("Route should be prepended with 'api/' prefix");
    }

    [Fact]
    public void PrependOperationWithRoutePrefix_False_DoesNotPrependPrefix()
    {
        var options = new SwaggerDocOptions
        {
            PrependOperationWithRoutePrefix = false,
            RoutePrefix = "api"
        };
        var provider = CreateProvider(options);

        var getItems = FindDescription(provider, "items", "GET");
        getItems.Should().NotBeNull("Route should NOT have prefix when PrependOperationWithRoutePrefix is false");

        var prefixed = FindDescription(provider, "api/items", "GET");
        prefixed.Should().BeNull("Route should NOT have prefix");
    }

    [Fact]
    public void HttpVerbs_CorrectlyMapped()
    {
        var provider = CreateProvider();

        var getItems = FindDescription(provider, "items", "GET");
        getItems.Should().NotBeNull();
        getItems!.HttpMethod.Should().Be("GET");

        var createItem = FindDescription(provider, "items", "POST");
        createItem.Should().NotBeNull();
        createItem!.HttpMethod.Should().Be("POST");
    }

    [Fact]
    public void MultipleVerbs_CreateMultipleDescriptions()
    {
        var provider = CreateProvider();

        var multiGet = FindDescription(provider, "multi", "GET");
        var multiPost = FindDescription(provider, "multi", "POST");

        multiGet.Should().NotBeNull("GET verb should create a description");
        multiPost.Should().NotBeNull("POST verb should create a description");
    }

    [Fact]
    public void AuthorizationLevelFunction_WithAddCodeParameter_AddsCodeQueryParam()
    {
        var options = new SwaggerDocOptions
        {
            AddCodeParameter = true,
            PrependOperationWithRoutePrefix = true
        };
        var provider = CreateProvider(options);

        var secure = FindDescription(provider, "secure", "GET");
        secure.Should().NotBeNull();

        var codeParam = secure!.ParameterDescriptions
            .FirstOrDefault(p => p.Name == "code");
        codeParam.Should().NotBeNull("AuthorizationLevel.Function should add 'code' query parameter");
        codeParam!.Source.Should().Be(BindingSource.Query);
    }

    [Fact]
    public void AuthorizationLevelAnonymous_DoesNotAddCodeParam()
    {
        var options = new SwaggerDocOptions
        {
            AddCodeParameter = true,
            PrependOperationWithRoutePrefix = true
        };
        var provider = CreateProvider(options);

        var getItems = FindDescription(provider, "items", "GET");
        getItems.Should().NotBeNull();

        var codeParam = getItems!.ParameterDescriptions
            .FirstOrDefault(p => p.Name == "code");
        codeParam.Should().BeNull("AuthorizationLevel.Anonymous should NOT add 'code' parameter");
    }

    [Fact]
    public void ControllerName_StripsControllerSuffix()
    {
        var provider = CreateProvider();

        var descriptions = provider.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .Where(d => d.RelativePath == "strip")
            .ToList();

        descriptions.Should().NotBeEmpty();
        var descriptor = descriptions[0].ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        descriptor.Should().NotBeNull();
        descriptor!.ControllerName.Should().Be("TestItems",
            "Class name 'TestItemsController' should have 'Controller' suffix stripped");
    }

    [Fact]
    public void OptionalRouteParameters_GenerateTwoRoutes()
    {
        var provider = CreateProvider();

        var allDescriptions = provider.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .ToList();

        // Optional parameter {name?} should generate two routes:
        // 1. "optional" (without the parameter)
        // 2. "optional/{name}" (with the parameter)
        var withoutParam = allDescriptions.FirstOrDefault(d =>
            d.RelativePath == "optional");
        var withParam = allDescriptions.FirstOrDefault(d =>
            d.RelativePath == "optional/{name}");

        withoutParam.Should().NotBeNull("Route without optional parameter should be generated");
        withParam.Should().NotBeNull("Route with optional parameter should be generated");
    }

    [Fact]
    public void RouteConstraints_AreStripped()
    {
        var provider = CreateProvider();

        // WithRouteConstraint has route "products/{id:int}" — the `:int` constraint should be stripped
        var withConstraint = FindDescription(provider, "products/{id}", "GET");
        withConstraint.Should().NotBeNull(
            "Route constraint ':int' should be stripped, leaving 'products/{id}'");

        // Verify the original route with constraint does NOT appear
        var withRawConstraint = provider.ApiDescriptionGroups.Items
            .SelectMany(g => g.Items)
            .FirstOrDefault(d => d.RelativePath != null && d.RelativePath.Contains(":int"));
        withRawConstraint.Should().BeNull("Route constraints should be fully stripped from paths");
    }

    [Fact]
    public void ResponseTypes_FromProducesResponseType_AreIncluded()
    {
        var provider = CreateProvider();

        var withResponses = FindDescription(provider, "responses", "GET");
        withResponses.Should().NotBeNull();

        withResponses!.SupportedResponseTypes.Should().HaveCount(2);
        withResponses.SupportedResponseTypes.Should().Contain(r => r.StatusCode == 200);
        withResponses.SupportedResponseTypes.Should().Contain(r => r.StatusCode == 404);
    }

    [Fact]
    public void AddCodeParameter_False_DoesNotAddCodeParamEvenForFunctionAuth()
    {
        var options = new SwaggerDocOptions
        {
            AddCodeParameter = false,
            PrependOperationWithRoutePrefix = true
        };
        var provider = CreateProvider(options);

        var secure = FindDescription(provider, "secure", "GET");
        secure.Should().NotBeNull();

        var codeParam = secure!.ParameterDescriptions
            .FirstOrDefault(p => p.Name == "code");
        codeParam.Should().BeNull("When AddCodeParameter=false, no 'code' parameter should be added");
    }

    [Fact]
    public void NonControllerClass_UsesFunction​NameAsControllerName()
    {
        var provider = CreateProvider();

        // TestControllerEndpoints doesn't end with "Controller", so function name is used
        var ctrl = FindDescription(provider, "ctrl", "GET");
        ctrl.Should().NotBeNull();

        var descriptor = ctrl!.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        descriptor.Should().NotBeNull();
        descriptor!.ControllerName.Should().Be("ControllerAction",
            "Non-Controller class should use function name as controller name");
    }

    [Fact]
    public void DefaultMediaType_IsApplicationJson()
    {
        var provider = CreateProvider();

        var getItems = FindDescription(provider, "items", "GET");
        getItems.Should().NotBeNull();

        getItems!.SupportedRequestFormats.Should().ContainSingle();
        getItems.SupportedRequestFormats[0].MediaType.Should().Be("application/json");
    }
}
