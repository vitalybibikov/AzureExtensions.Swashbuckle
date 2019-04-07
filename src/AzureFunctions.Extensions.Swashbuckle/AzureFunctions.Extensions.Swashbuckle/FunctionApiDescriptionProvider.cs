using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureFunctions.Extensions.Swashbuckle
{
    internal class FunctionApiDescriptionProvider : IApiDescriptionGroupCollectionProvider
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly IOutputFormatter _outputFormatter;

        public FunctionApiDescriptionProvider(
            IOptions<Option> functionsOptions,
            SwashBuckleStartupConfig startupConfig,
            IModelMetadataProvider modelMetadataProvider, 
            IOutputFormatter outputFormatter, 
            IOptions<HttpOptions> httOptions)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _outputFormatter = outputFormatter;

            var methods = startupConfig.Assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(FunctionNameAttribute), false).Any())
                .ToArray();

            IList<ApiDescriptionGroup> apiDescrGroup = new List<ApiDescriptionGroup>();
            foreach (var methodInfo in methods)
            {
                if (!TryGetHttpTrigger(methodInfo, out var triggerAttribute))
                    continue;

                var functionAttr =
                    (FunctionNameAttribute)methodInfo.GetCustomAttribute(typeof(FunctionNameAttribute), false);
                var prefix = string.IsNullOrWhiteSpace(httOptions.Value.RoutePrefix) ? "" : $"{httOptions.Value.RoutePrefix.TrimEnd('/')}/";
                var route =
                    $"{prefix}{(!string.IsNullOrWhiteSpace(triggerAttribute.Route) ? triggerAttribute.Route : functionAttr.Name)}";
                var verbs = triggerAttribute.Methods ??
                            new[] { "get", "post", "delete", "head", "patch", "put", "options" };

                var items = verbs.Select(verb => CreateDescription(methodInfo, route, functionAttr, verb)).ToArray();                
                var group = new ApiDescriptionGroup(functionAttr.Name, items);
                apiDescrGroup.Add(group);
            }

            ApiDescriptionGroups =
                new ApiDescriptionGroupCollection(new ReadOnlyCollection<ApiDescriptionGroup>(apiDescrGroup), 1);
        }

        public ApiDescriptionGroupCollection ApiDescriptionGroups { get; }

        private bool TryGetHttpTrigger(MethodInfo methodInfo, out HttpTriggerAttribute triggerAttribute)
        {
            triggerAttribute = null;
            var ignore = methodInfo.GetCustomAttributes().Any(x => x is SwaggerIgnoreAttribute);
            if (ignore)
                return false;

            triggerAttribute = FindHttpTriggerAttribute(methodInfo);
            if (triggerAttribute == null)
                return false;

            return true;
        }

        private ApiDescription CreateDescription(MethodInfo methodInfo, string route,
            FunctionNameAttribute functionAttr,
            string verb)
        {
            var controlleName = methodInfo.DeclaringType.Name.EndsWith("Controller")
                ? methodInfo.DeclaringType.Name.Remove(
                    methodInfo.DeclaringType.Name.Length - "Controller".Length, "Controller".Length)
                : functionAttr.Name;
            var actionName = functionAttr.Name;

            var description = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    MethodInfo = methodInfo,
                    ControllerName = controlleName,
                    DisplayName = actionName,
                    ControllerTypeInfo = methodInfo.DeclaringType.GetTypeInfo(),
                    Parameters = new List<ParameterDescriptor>(),
                    RouteValues = new Dictionary<string, string>()
                    {
                        {"controller",controlleName },
                        {"action", actionName }
                    }
                },
                RelativePath = route,
                HttpMethod = verb.ToUpper()
            };

            var supportedMediaTypes = methodInfo.GetCustomAttributes<SupportedRequestFormatAttribute>()
                .Select(x => new ApiRequestFormat {MediaType = x.MediaType}).ToList();
            foreach (var supportedMediaType in supportedMediaTypes)
                description.SupportedRequestFormats.Add(supportedMediaType);

            var parameters = GetParametersDescription(methodInfo, route).ToList();
            foreach (var parameter in parameters)
            {
                description.ActionDescriptor.Parameters.Add(new ParameterDescriptor
                {
                    Name = parameter.Name,
                    ParameterType = parameter.Type
                });
                description.ParameterDescriptions.Add(parameter);
            }

            foreach (var apiResponseType in GetResponseTypes(methodInfo))
            {
                description.SupportedResponseTypes.Add(apiResponseType);
            }

            return description;
        }

        private IEnumerable<ApiResponseType> GetResponseTypes(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(ProducesResponseTypeAttribute))
                .Select(customAttribute => customAttribute as ProducesResponseTypeAttribute)
                .Select(responseType => new ApiResponseType
                {
                    ApiResponseFormats = new[]
                    {
                        new ApiResponseFormat
                        {
                            Formatter = _outputFormatter,
                            MediaType = "application/json"
                        }
                    },
                    ModelMetadata = _modelMetadataProvider.GetMetadataForType(responseType.Type),
                    Type = responseType.Type,
                    StatusCode = responseType.StatusCode
                });
        }

        private HttpTriggerAttribute FindHttpTriggerAttribute(MethodInfo methodInfo)
        {
            HttpTriggerAttribute triggerAttribute = null;
            foreach (var parameter in methodInfo.GetParameters())
            {
                triggerAttribute = parameter.GetCustomAttributes(typeof(HttpTriggerAttribute), false)
                    .FirstOrDefault() as HttpTriggerAttribute;
                if (triggerAttribute != null)
                    break;
            }

            return triggerAttribute;
        }

        private IEnumerable<ApiParameterDescription> GetParametersDescription(MethodInfo methodInfo, string route)
        {
            foreach (var parameter in methodInfo.GetParameters())
            {
                var requestBodyTypeAttribute =
                    parameter.GetCustomAttribute(typeof(RequestBodyTypeAttribute)) as RequestBodyTypeAttribute;

                if ((parameter.ParameterType == typeof(HttpRequestMessage) || parameter.ParameterType == typeof(HttpRequest))
                    && requestBodyTypeAttribute == null)
                    continue;

                if (IgnoreParameter(parameter))
                    continue;

                var hasHttpTrigerAttribute = parameter.GetCustomAttributes().Any(attr => attr is HttpTriggerAttribute);
                var hasFromUriAttribute =
                    false; // parameter.GetCustomAttributes().Any(attr => attr is FromUriAttribute);

                var type = hasHttpTrigerAttribute && requestBodyTypeAttribute != null
                    ? requestBodyTypeAttribute.Type
                    : parameter.ParameterType;

                var bindingSource = route.Contains("{" + parameter.Name) ? BindingSource.Path
                    : hasFromUriAttribute ? BindingSource.Query
                    : BindingSource.Body;

                yield return new ApiParameterDescription
                {
                    Name = parameter.Name,
                    Type = type,
                    Source = bindingSource,
                    RouteInfo = new ApiParameterRouteInfo
                    {
                        IsOptional = bindingSource == BindingSource.Query
                    }
                };
            }
        }

        private bool IgnoreParameter(ParameterInfo parameter)
        {
            var ignoreParameterAttribute = parameter.GetCustomAttribute(typeof(SwaggerIgnoreAttribute));
            if (ignoreParameterAttribute != null) return true;
            if (parameter.ParameterType.Name == "TraceWriter") return true;
            if (parameter.ParameterType == typeof(ExecutionContext)) return true;
            if (parameter.ParameterType == typeof(ILogger)) return true;
            if (parameter.ParameterType.IsAssignableFrom(typeof(ILogger))) return true;
            if (parameter.ParameterType.IsAssignableFrom(typeof(ISwashBuckleClient))) return true;
            if (parameter.GetCustomAttributes().Any(attr =>(attr is HttpTriggerAttribute)) 
                && parameter.GetCustomAttributes().All(attr => !(attr is RequestBodyTypeAttribute))) return true;
            return false;
        }
    }
}