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

public class FileUploadOperationFilterTests
{
    private readonly FileUploadOperationFilter _filter = new();

    [Fact]
    public void Apply_MethodWithSwaggerUploadFileAttribute_ProducesMultipartSchema()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestEndpoints.UploadFile));

        _filter.Apply(operation, context);

        operation.RequestBody.Should().NotBeNull();
        operation.RequestBody!.Content.Should().ContainKey("multipart/form-data");

        var mediaType = operation.RequestBody.Content["multipart/form-data"];
        mediaType.Schema.Should().NotBeNull();
        mediaType.Schema!.Type.Should().Be(JsonSchemaType.Object);
        mediaType.Schema.Properties.Should().ContainKey("uploadedFile");

        var fileProperty = mediaType.Schema.Properties["uploadedFile"] as OpenApiSchema;
        fileProperty.Should().NotBeNull();
        fileProperty!.Type.Should().Be(JsonSchemaType.String);
        fileProperty.Format.Should().Be("binary");
        fileProperty.Description.Should().Be("File to upload.");
    }

    [Fact]
    public void Apply_MethodWithoutAttribute_IsNoOp()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestEndpoints.NoUpload));

        _filter.Apply(operation, context);

        operation.RequestBody.Should().BeNull();
    }

    [Fact]
    public void Apply_MethodWithCustomFileNameAndDescription_UsesCustomValues()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestEndpoints.CustomUpload));

        _filter.Apply(operation, context);

        operation.RequestBody.Should().NotBeNull();
        var mediaType = operation.RequestBody!.Content["multipart/form-data"];
        mediaType.Schema!.Properties.Should().ContainKey("myDocument");
        mediaType.Schema.Required.Should().Contain("myDocument");

        var fileProperty = mediaType.Schema.Properties["myDocument"] as OpenApiSchema;
        fileProperty.Should().NotBeNull();
        fileProperty!.Description.Should().Be("PDF document to process");
    }

    [Fact]
    public void Apply_MethodWithExampleSet_SetsExampleOnSchema()
    {
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestEndpoints.UploadWithExample));

        _filter.Apply(operation, context);

        operation.RequestBody.Should().NotBeNull();
        var mediaType = operation.RequestBody!.Content["multipart/form-data"];
        var schema = mediaType.Schema as OpenApiSchema;
        schema.Should().NotBeNull();
        schema!.Example.Should().NotBeNull();
    }

    private static OperationFilterContext CreateOperationFilterContext(string methodName)
    {
        var methodInfo = typeof(TestEndpoints).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)!;
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

    private class TestEndpoints
    {
        [SwaggerUploadFile("uploadedFile", "File to upload.")]
        public void UploadFile() { }

        public void NoUpload() { }

        [SwaggerUploadFile("myDocument", "PDF document to process")]
        public void CustomUpload() { }

        [SwaggerUploadFile("report", "Report file", "sample.pdf")]
        public void UploadWithExample() { }
    }
}
