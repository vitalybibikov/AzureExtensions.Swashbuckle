using System;
using System.Reflection;
using System.Xml.XPath;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters.Mapper;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    public class XmlCommentsSchemaFilterChanged : ISchemaFilter
    {
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsSchemaFilterChanged(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator() ?? throw new ArgumentException();
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            ApplyTypeTags(schema, context.Type);

            if (context.MemberInfo != null && context.ParameterInfo == null)
            {
                ApplyFieldOrPropertyTags(schema, context.MemberInfo);
            }
        }

        private void ApplyTypeTags(OpenApiSchema schema, Type type)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
            var typeSummaryNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{typeMemberName}']/summary");

            if (typeSummaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(typeSummaryNode.InnerXml);
            }
        }

        private void ApplyFieldOrPropertyTags(OpenApiSchema schema, MemberInfo fieldOrPropertyInfo)
        {
            var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(fieldOrPropertyInfo);
            var fieldOrPropertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{fieldOrPropertyMemberName}']");

            if (fieldOrPropertyNode == null) return;

            var summaryNode = fieldOrPropertyNode.SelectSingleNode("summary");
            if (summaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            var exampleNode = fieldOrPropertyNode.SelectSingleNode("example");
            if (exampleNode != null)
            {
                schema.Example = JsonMapper.CreateFromJson(exampleNode.InnerXml);
            }
        }
    }
}
