// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class GenerateOperationIdFilterTests
{
    private readonly GenerateOperationIdFilter _filter = new();

    [Fact]
    public void Apply_WithControllerActionDescriptorAndActionName_SetsOperationId()
    {
        var operation = new OpenApiOperation();
        var context = CreateContextWithControllerDescriptor("GetItems");

        _filter.Apply(operation, context);

        operation.OperationId.Should().Be("GetItems");
    }

    [Fact]
    public void Apply_WithControllerActionDescriptorAndEmptyActionName_DoesNotSetOperationId()
    {
        var operation = new OpenApiOperation();
        var context = CreateContextWithControllerDescriptor(string.Empty);

        _filter.Apply(operation, context);

        operation.OperationId.Should().BeNull();
    }

    [Fact]
    public void Apply_WithControllerActionDescriptorAndNullActionName_DoesNotSetOperationId()
    {
        var operation = new OpenApiOperation();
        var context = CreateContextWithControllerDescriptor(null);

        _filter.Apply(operation, context);

        operation.OperationId.Should().BeNull();
    }

    [Fact]
    public void Apply_WithNonControllerActionDescriptor_DoesNotSetOperationId()
    {
        var operation = new OpenApiOperation();
        var apiDescription = new ApiDescription();
        // Default ActionDescriptor is not ControllerActionDescriptor
        apiDescription.ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor();

        var context = CreateContextFromApiDescription(apiDescription);

        _filter.Apply(operation, context);

        operation.OperationId.Should().BeNull();
    }

    [Fact]
    public void Apply_PreservesExistingOperationIdWhenActionNameIsEmpty()
    {
        var operation = new OpenApiOperation { OperationId = "ExistingId" };
        var context = CreateContextWithControllerDescriptor(string.Empty);

        _filter.Apply(operation, context);

        operation.OperationId.Should().Be("ExistingId");
    }

    private static OperationFilterContext CreateContextWithControllerDescriptor(string? actionName)
    {
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                ActionName = actionName!,
                ControllerName = "Test",
                MethodInfo = typeof(GenerateOperationIdFilterTests).GetMethod(
                    nameof(CreateContextWithControllerDescriptor),
                    BindingFlags.NonPublic | BindingFlags.Static)!,
                ControllerTypeInfo = typeof(GenerateOperationIdFilterTests).GetTypeInfo()
            }
        };

        return CreateContextFromApiDescription(apiDescription);
    }

    private static OperationFilterContext CreateContextFromApiDescription(ApiDescription apiDescription)
    {
        var schemaGeneratorOptions = new SchemaGeneratorOptions();
        var schemaGenerator = new SchemaGenerator(
            schemaGeneratorOptions,
            new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));
        var schemaRepository = new SchemaRepository();
        var document = new OpenApiDocument();

        return new OperationFilterContext(
            apiDescription,
            schemaGenerator,
            schemaRepository,
            document,
            typeof(GenerateOperationIdFilterTests).GetMethod(
                nameof(CreateContextFromApiDescription),
                BindingFlags.NonPublic | BindingFlags.Static)!);
    }
}
