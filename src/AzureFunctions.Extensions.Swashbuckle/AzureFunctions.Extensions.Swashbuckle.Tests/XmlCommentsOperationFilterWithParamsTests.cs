// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Reflection;
using System.Text;
using System.Xml.XPath;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class XmlCommentsOperationFilterWithParamsTests
{
    [Fact]
    public void Apply_WithMethodSummary_SetsOperationSummary()
    {
        var methodInfo = typeof(TestXmlEndpoints).GetMethod(nameof(TestXmlEndpoints.GetItems))!;
        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{methodMemberName}"">
      <summary>Gets all items from the store.</summary>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsOperationFilterWithParams(xmlDoc);
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestXmlEndpoints.GetItems));

        filter.Apply(operation, context);

        operation.Summary.Should().Be("Gets all items from the store.");
    }

    [Fact]
    public void Apply_WithMethodRemarks_SetsOperationDescription()
    {
        var methodInfo = typeof(TestXmlEndpoints).GetMethod(nameof(TestXmlEndpoints.GetItems))!;
        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{methodMemberName}"">
      <summary>Gets items.</summary>
      <remarks>This endpoint returns all available items.</remarks>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsOperationFilterWithParams(xmlDoc);
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestXmlEndpoints.GetItems));

        filter.Apply(operation, context);

        operation.Summary.Should().Be("Gets items.");
        operation.Description.Should().Be("This endpoint returns all available items.");
    }

    [Fact]
    public void Apply_WithParameterDescription_SetsParameterDescription()
    {
        var methodInfo = typeof(TestXmlEndpoints).GetMethod(nameof(TestXmlEndpoints.GetItemById))!;
        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{methodMemberName}"">
      <summary>Gets an item by ID.</summary>
      <param name=""id"">The unique identifier of the item.</param>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsOperationFilterWithParams(xmlDoc);
        var operation = new OpenApiOperation
        {
            Parameters = new List<IOpenApiParameter>
            {
                new OpenApiParameter { Name = "id" }
            }
        };
        var context = CreateOperationFilterContext(nameof(TestXmlEndpoints.GetItemById));

        filter.Apply(operation, context);

        operation.Summary.Should().Be("Gets an item by ID.");
        var param = operation.Parameters[0] as OpenApiParameter;
        param!.Description.Should().Be("The unique identifier of the item.");
    }

    [Fact]
    public void Apply_WithResponseTags_AddsResponsesToOperation()
    {
        var methodInfo = typeof(TestXmlEndpoints).GetMethod(nameof(TestXmlEndpoints.GetItems))!;
        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{methodMemberName}"">
      <summary>Gets items.</summary>
      <response code=""200"">Returns the list of items.</response>
      <response code=""404"">Items not found.</response>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsOperationFilterWithParams(xmlDoc);
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestXmlEndpoints.GetItems));

        filter.Apply(operation, context);

        operation.Responses.Should().ContainKey("200");
        operation.Responses["200"].Description.Should().Be("Returns the list of items.");
        operation.Responses.Should().ContainKey("404");
        operation.Responses["404"].Description.Should().Be("Items not found.");
    }

    [Fact]
    public void Apply_WithControllerResponseTags_AddsControllerResponses()
    {
        var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(typeof(TestXmlEndpoints));
        var methodInfo = typeof(TestXmlEndpoints).GetMethod(nameof(TestXmlEndpoints.GetItems))!;
        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{typeMemberName}"">
      <response code=""401"">Unauthorized access.</response>
    </member>
    <member name=""{methodMemberName}"">
      <summary>Gets items.</summary>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsOperationFilterWithParams(xmlDoc);
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestXmlEndpoints.GetItems));

        filter.Apply(operation, context);

        operation.Responses.Should().ContainKey("401");
        operation.Responses["401"].Description.Should().Be("Unauthorized access.");
    }

    [Fact]
    public void Apply_WithNullMethodInfo_DoesNotThrow()
    {
        var xmlDoc = CreateXPathDocument(@"
<doc>
  <members>
  </members>
</doc>");

        var filter = new XmlCommentsOperationFilterWithParams(xmlDoc);
        var operation = new OpenApiOperation();

        var schemaGenerator = new SchemaGenerator(
            new SchemaGeneratorOptions(),
            new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));
        var context = new OperationFilterContext(
            new ApiDescription(),
            schemaGenerator,
            new SchemaRepository(),
            new OpenApiDocument(),
            methodInfo: null!);

        var act = () => filter.Apply(operation, context);

        act.Should().NotThrow();
    }

    [Fact]
    public void Apply_WithMissingMethodNode_DoesNotSetSummaryOrDescription()
    {
        var xmlDoc = CreateXPathDocument(@"
<doc>
  <members>
  </members>
</doc>");

        var filter = new XmlCommentsOperationFilterWithParams(xmlDoc);
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestXmlEndpoints.GetItems));

        filter.Apply(operation, context);

        operation.Summary.Should().BeNull();
        operation.Description.Should().BeNull();
    }

    [Fact]
    public void Apply_ParameterNotInOperation_DoesNotThrow()
    {
        var methodInfo = typeof(TestXmlEndpoints).GetMethod(nameof(TestXmlEndpoints.GetItemById))!;
        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{methodMemberName}"">
      <param name=""id"">The ID.</param>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsOperationFilterWithParams(xmlDoc);
        var operation = new OpenApiOperation();
        var context = CreateOperationFilterContext(nameof(TestXmlEndpoints.GetItemById));

        var act = () => filter.Apply(operation, context);

        act.Should().NotThrow();
    }

    private static XPathDocument CreateXPathDocument(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return new XPathDocument(stream);
    }

    private static OperationFilterContext CreateOperationFilterContext(string methodName)
    {
        var methodInfo = typeof(TestXmlEndpoints).GetMethod(
            methodName, BindingFlags.Public | BindingFlags.Instance)!;
        var schemaGenerator = new SchemaGenerator(
            new SchemaGeneratorOptions(),
            new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));

        return new OperationFilterContext(
            new ApiDescription(),
            schemaGenerator,
            new SchemaRepository(),
            new OpenApiDocument(),
            methodInfo);
    }

    // Test endpoint stubs
    public class TestXmlEndpoints
    {
        public void GetItems() { }

        public void GetItemById(string id) { }
    }
}
