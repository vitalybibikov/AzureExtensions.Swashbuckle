// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters;
using FluentAssertions;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class FunctionsOperationFilterTests
{
    private readonly FunctionsOperationFilter _filter = new();

    [Fact]
    public void Apply_MethodWithRequestHttpHeaderAttribute_AddsHeaderParameter()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(
            typeof(MethodLevelHeaderEndpoints),
            nameof(MethodLevelHeaderEndpoints.WithHeader));

        _filter.Apply(operation, context);

        operation.Parameters.Should().HaveCount(1);
        var param = operation.Parameters[0] as OpenApiParameter;
        param.Should().NotBeNull();
        param!.Name.Should().Be("X-Custom-Header");
        param.In.Should().Be(ParameterLocation.Header);
        param.Schema.Should().NotBeNull();
        (param.Schema as OpenApiSchema)!.Type.Should().Be(JsonSchemaType.String);
        param.Required.Should().BeFalse();
    }

    [Fact]
    public void Apply_ClassLevelRequestHttpHeaderAttribute_AddsHeaderParameter()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(
            typeof(ClassLevelHeaderEndpoints),
            nameof(ClassLevelHeaderEndpoints.SomeMethod));

        _filter.Apply(operation, context);

        operation.Parameters.Should().ContainSingle();
        var param = operation.Parameters[0] as OpenApiParameter;
        param.Should().NotBeNull();
        param!.Name.Should().Be("X-Api-Key");
        param.In.Should().Be(ParameterLocation.Header);
        param.Required.Should().BeTrue();
    }

    [Fact]
    public void Apply_RequiredHeader_SetsRequiredTrue()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(
            typeof(RequiredHeaderEndpoints),
            nameof(RequiredHeaderEndpoints.RequiredHeaderMethod));

        _filter.Apply(operation, context);

        operation.Parameters.Should().HaveCount(1);
        var param = operation.Parameters[0] as OpenApiParameter;
        param!.Required.Should().BeTrue();
    }

    [Fact]
    public void Apply_OptionalHeader_SetsRequiredFalse()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(
            typeof(MethodLevelHeaderEndpoints),
            nameof(MethodLevelHeaderEndpoints.WithHeader));

        _filter.Apply(operation, context);

        var param = operation.Parameters[0] as OpenApiParameter;
        param!.Required.Should().BeFalse();
    }

    [Fact]
    public void Apply_MultipleMethodHeaders_AddsAllParameters()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(
            typeof(MultipleHeaderEndpoints),
            nameof(MultipleHeaderEndpoints.MultipleHeaders));

        _filter.Apply(operation, context);

        operation.Parameters.Should().HaveCount(2);
        operation.Parameters.Cast<OpenApiParameter>().Select(p => p.Name)
            .Should().Contain(new[] { "X-First", "X-Second" });
    }

    [Fact]
    public void Apply_MethodAndClassHeaders_AddsBoth()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(
            typeof(ClassLevelHeaderEndpoints),
            nameof(ClassLevelHeaderEndpoints.MethodWithAdditionalHeader));

        _filter.Apply(operation, context);

        operation.Parameters.Should().HaveCount(2);
        operation.Parameters.Cast<OpenApiParameter>().Select(p => p.Name)
            .Should().Contain(new[] { "X-Api-Key", "X-Extra" });
    }

    [Fact]
    public void Apply_NoAttributes_ParametersListIsEmpty()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(
            typeof(NoHeaderEndpoints),
            nameof(NoHeaderEndpoints.PlainMethod));

        _filter.Apply(operation, context);

        operation.Parameters.Should().BeEmpty();
    }

    private static OperationFilterContext CreateOperationFilterContext(Type declaringType, string methodName)
    {
        var methodInfo = declaringType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)!;
        var schemaGeneratorOptions = new SchemaGeneratorOptions();
        var schemaGenerator = new SchemaGenerator(schemaGeneratorOptions, new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));
        var schemaRepository = new SchemaRepository();
        var document = new OpenApiDocument();

        return new OperationFilterContext(
            new Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription(),
            schemaGenerator,
            schemaRepository,
            document,
            methodInfo);
    }

    // Test endpoint stubs

    private class MethodLevelHeaderEndpoints
    {
        [RequestHttpHeader("X-Custom-Header")]
        public void WithHeader() { }
    }

    [RequestHttpHeader("X-Api-Key", isRequired: true)]
    private class ClassLevelHeaderEndpoints
    {
        public void SomeMethod() { }

        [RequestHttpHeader("X-Extra")]
        public void MethodWithAdditionalHeader() { }
    }

    private class RequiredHeaderEndpoints
    {
        [RequestHttpHeader("Authorization", isRequired: true)]
        public void RequiredHeaderMethod() { }
    }

    private class MultipleHeaderEndpoints
    {
        [RequestHttpHeader("X-First")]
        [RequestHttpHeader("X-Second", isRequired: true)]
        public void MultipleHeaders() { }
    }

    private class NoHeaderEndpoints
    {
        public void PlainMethod() { }
    }
}
