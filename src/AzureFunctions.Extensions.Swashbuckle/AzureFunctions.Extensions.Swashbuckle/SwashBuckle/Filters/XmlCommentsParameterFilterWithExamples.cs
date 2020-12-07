using System;
using System.Reflection;
using System.Xml.XPath;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters.Mapper;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    public class XmlCommentsParameterFilterWithExamples : IParameterFilter
    {
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsParameterFilterWithExamples(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator() ?? throw new ArgumentException();
        }

        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (context.PropertyInfo != null)
            {
                ApplyPropertyTags(parameter, context.PropertyInfo);
            }
            else if (context.ParameterInfo != null)
            {
                ApplyParamTags(parameter, context.ParameterInfo);
            }
        }

        private void ApplyPropertyTags(OpenApiParameter parameter, PropertyInfo propertyInfo)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(propertyInfo);
            var propertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']");

            if (propertyNode == null) return;

            var summaryNode = propertyNode.SelectSingleNode("summary");
            if (summaryNode != null)
                parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var exampleNode = propertyNode.SelectSingleNode("example");
            if (exampleNode != null)
            {
                parameter.Example = JsonMapper.CreateFromJson(exampleNode.InnerXml);
            }
        }

        private void ApplyParamTags(OpenApiParameter parameter, ParameterInfo parameterInfo)
        {
            if (!(parameterInfo.Member is MethodInfo methodInfo)) return;

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = methodInfo.DeclaringType.IsConstructedGenericType
                ? methodInfo.GetUnderlyingGenericTypeMethod()
                : methodInfo;

            if (targetMethod == null) return;

            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(targetMethod);
            var paramNode = _xmlNavigator.SelectSingleNode(
                $"/doc/members/member[@name='{methodMemberName}']/param[@name='{parameterInfo.Name}']");

            if (paramNode != null)
            {
                parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

                var example = paramNode.GetAttribute("example", "");
                if (!string.IsNullOrEmpty(example))
                {
                    parameter.Example = JsonMapper.CreateFromJson(example);
                }
            }
        }
    }
}
