// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Text;
using System.Xml.XPath;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters;
using FluentAssertions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class XmlCommentsSchemaFilterChangedTests
{
    [Fact]
    public void Apply_WithTypeSummary_SetsSchemaDescription()
    {
        var typeName = XmlCommentsNodeNameHelper.GetMemberNameForType(typeof(TestSchemaModel));
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{typeName}"">
      <summary>A test model for schema filtering.</summary>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsSchemaFilterChanged(xmlDoc);
        var schema = new OpenApiSchema();
        var context = CreateSchemaFilterContext(typeof(TestSchemaModel));

        filter.Apply(schema, context);

        schema.Description.Should().Be("A test model for schema filtering.");
    }

    [Fact]
    public void Apply_WithPropertySummaryAndExample_SetsDescriptionAndExample()
    {
        var memberInfo = typeof(TestSchemaModel).GetProperty(nameof(TestSchemaModel.Name))!;
        var memberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(memberInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{memberName}"">
      <summary>The name of the item.</summary>
      <example>Widget</example>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsSchemaFilterChanged(xmlDoc);
        var schema = new OpenApiSchema();
        var context = CreateSchemaFilterContext(typeof(string), memberInfo: memberInfo);

        filter.Apply(schema, context);

        schema.Description.Should().Be("The name of the item.");
        schema.Example.Should().NotBeNull();
    }

    [Fact]
    public void Apply_WithPropertySummaryOnly_SetsDescriptionWithoutExample()
    {
        var memberInfo = typeof(TestSchemaModel).GetProperty(nameof(TestSchemaModel.Name))!;
        var memberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(memberInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{memberName}"">
      <summary>Just a description.</summary>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsSchemaFilterChanged(xmlDoc);
        var schema = new OpenApiSchema();
        var context = CreateSchemaFilterContext(typeof(string), memberInfo: memberInfo);

        filter.Apply(schema, context);

        schema.Description.Should().Be("Just a description.");
        schema.Example.Should().BeNull();
    }

    [Fact]
    public void Apply_WithMissingXmlNode_DoesNotCrash()
    {
        var xmlDoc = CreateXPathDocument(@"
<doc>
  <members>
  </members>
</doc>");

        var filter = new XmlCommentsSchemaFilterChanged(xmlDoc);
        var schema = new OpenApiSchema();
        var context = CreateSchemaFilterContext(typeof(TestSchemaModel));

        var act = () => filter.Apply(schema, context);

        act.Should().NotThrow();
        schema.Description.Should().BeNull();
    }

    [Fact]
    public void Apply_WithMissingPropertyNode_DoesNotCrash()
    {
        var xmlDoc = CreateXPathDocument(@"
<doc>
  <members>
  </members>
</doc>");

        var filter = new XmlCommentsSchemaFilterChanged(xmlDoc);
        var schema = new OpenApiSchema();
        var memberInfo = typeof(TestSchemaModel).GetProperty(nameof(TestSchemaModel.Name))!;
        var context = CreateSchemaFilterContext(typeof(string), memberInfo: memberInfo);

        var act = () => filter.Apply(schema, context);

        act.Should().NotThrow();
        schema.Description.Should().BeNull();
    }

    [Fact]
    public void Apply_WhenParameterInfoIsNotNull_DoesNotApplyFieldOrPropertyTags()
    {
        var memberInfo = typeof(TestSchemaModel).GetProperty(nameof(TestSchemaModel.Name))!;
        var memberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(memberInfo);
        var xmlDoc = CreateXPathDocument($@"
<doc>
  <members>
    <member name=""{memberName}"">
      <summary>Should not be applied.</summary>
    </member>
  </members>
</doc>");

        var filter = new XmlCommentsSchemaFilterChanged(xmlDoc);
        var schema = new OpenApiSchema();

        var methodInfo = typeof(TestSchemaModel).GetMethod(nameof(TestSchemaModel.SomeMethod))!;
        var parameterInfo = methodInfo.GetParameters()[0];
        var context = CreateSchemaFilterContext(typeof(string), memberInfo: memberInfo, parameterInfo: parameterInfo);

        filter.Apply(schema, context);

        schema.Description.Should().BeNull();
    }

    private static XPathDocument CreateXPathDocument(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return new XPathDocument(stream);
    }

    private static SchemaFilterContext CreateSchemaFilterContext(
        Type type,
        System.Reflection.MemberInfo? memberInfo = null,
        System.Reflection.ParameterInfo? parameterInfo = null)
    {
        var schemaGeneratorOptions = new SchemaGeneratorOptions();
        var schemaGenerator = new SchemaGenerator(
            schemaGeneratorOptions,
            new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));
        var schemaRepository = new SchemaRepository();

        return new SchemaFilterContext(
            type,
            schemaGenerator,
            schemaRepository,
            memberInfo: memberInfo,
            parameterInfo: parameterInfo);
    }

    public class TestSchemaModel
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }

        public void SomeMethod(string input) { }
    }
}
