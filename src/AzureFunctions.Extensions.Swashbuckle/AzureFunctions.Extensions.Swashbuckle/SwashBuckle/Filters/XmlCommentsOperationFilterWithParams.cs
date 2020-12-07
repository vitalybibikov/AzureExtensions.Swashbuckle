using System;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    public class XmlCommentsOperationFilterWithParams : IOperationFilter
    {
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsOperationFilterWithParams(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator() ?? throw new ArgumentException();
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null)
            {
                return;
            }

            var targetMethod = context.MethodInfo.DeclaringType!.IsConstructedGenericType
                ? context.MethodInfo.GetUnderlyingGenericTypeMethod()
                : context.MethodInfo;

            if (targetMethod == null)
            {
                return;
            }

            ApplyParameters(operation, targetMethod);
            ApplyControllerTags(operation, targetMethod.DeclaringType);
            ApplyMethodTags(operation, targetMethod);
        }

        private void ApplyParameters(OpenApiOperation operation, MethodInfo methodInfo)
        {
            if (methodInfo != null)
            {   
                var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
                foreach (var parameter in methodInfo.GetParameters())
                {
                    if (!String.IsNullOrEmpty(parameter.Name))
                    {
                        var paramNode = _xmlNavigator.SelectSingleNode(
                            $"/doc/members/member[@name='{methodMemberName}']/param[@name='{parameter.Name}']");

                        if (paramNode != null)
                        {
                            var humanizedDescription =  XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

                            var operationParameter = operation.Parameters
                                .FirstOrDefault(x => x.Name == parameter.Name);

                            if (operationParameter != null)
                            {
                                operationParameter.Description = humanizedDescription;
                            }
                        }
                    }
                }
            }
        }

        private void ApplyControllerTags(OpenApiOperation operation, Type controllerType)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(controllerType);
            var responseNodes = _xmlNavigator.Select($"/doc/members/member[@name='{typeMemberName}']/response");
            ApplyResponseTags(operation, responseNodes);
        }

        private void ApplyMethodTags(OpenApiOperation operation, MethodInfo methodInfo)
        {
            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
            var methodNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{methodMemberName}']");

            if (methodNode == null) return;

            var summaryNode = methodNode.SelectSingleNode("summary");
            if (summaryNode != null)
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var remarksNode = methodNode.SelectSingleNode("remarks");
            if (remarksNode != null)
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);

            var responseNodes = methodNode.Select("response");
            ApplyResponseTags(operation, responseNodes);
        }

        
        private void ApplyResponseTags(OpenApiOperation operation, XPathNodeIterator responseNodes)
        {
            while (responseNodes.MoveNext())
            {
                var code = responseNodes.Current.GetAttribute("code", "");
                var response = operation.Responses.ContainsKey(code)
                    ? operation.Responses[code]
                    : operation.Responses[code] = new OpenApiResponse();

                response.Description = XmlCommentsTextHelper.Humanize(responseNodes.Current.InnerXml);
            }
        }
    }
}
