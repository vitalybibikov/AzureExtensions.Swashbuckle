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
    }
}
