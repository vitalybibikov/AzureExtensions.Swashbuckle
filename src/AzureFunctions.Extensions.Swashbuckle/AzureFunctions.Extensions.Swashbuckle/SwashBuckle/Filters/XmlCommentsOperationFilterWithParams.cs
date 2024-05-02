using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    public class XmlCommentsOperationFilterWithParams : IOperationFilter
    {
        private readonly XPathNavigator xmlNavigator;

        public XmlCommentsOperationFilterWithParams(XPathDocument xmlDoc)
        {
            this.xmlNavigator = xmlDoc.CreateNavigator() ?? throw new ArgumentException();
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

            this.ApplyParameters(operation, targetMethod);
            this.ApplyControllerTags(operation, targetMethod.DeclaringType!);
            this.ApplyMethodTags(operation, targetMethod);
        }

        private void ApplyParameters(OpenApiOperation operation, MethodInfo methodInfo)
        {
            if (methodInfo != null)
            {   
                var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
                foreach (var parameter in methodInfo.GetParameters())
                {
                    if (!string.IsNullOrEmpty(parameter.Name))
                    {
                        var paramNode = this.xmlNavigator.SelectSingleNode(
                            $"/doc/members/member[@name='{methodMemberName}']/param[@name='{parameter.Name}']");

                        if (paramNode != null)
                        {
                            var humanizedDescription = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

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
            var responseNodes = this.xmlNavigator.Select($"/doc/members/member[@name='{typeMemberName}']/response");
            this.ApplyResponseTags(operation, responseNodes);
        }

        private void ApplyMethodTags(OpenApiOperation operation, MethodInfo methodInfo)
        {
            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
            var methodNode = this.xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{methodMemberName}']");

            if (methodNode == null)
            {
                return;
            }

            var summaryNode = methodNode.SelectSingleNode("summary");
            if (summaryNode != null)
            {
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            var remarksNode = methodNode.SelectSingleNode("remarks");
            if (remarksNode != null)
            {
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);
            }
            
            var responseNodes = methodNode.Select("response");
            this.ApplyResponseTags(operation, responseNodes);
        }

        private void ApplyResponseTags(OpenApiOperation operation, XPathNodeIterator responseNodes)
        {
            while (responseNodes.MoveNext())
            {
                var code = responseNodes.Current!.GetAttribute("code", string.Empty);
                var response = operation.Responses.TryGetValue(code, out var operationResponse)
                    ? operationResponse
                    : operation.Responses[code] = new OpenApiResponse();

                response.Description = XmlCommentsTextHelper.Humanize(responseNodes.Current.InnerXml);
            }
        }
    }
}
