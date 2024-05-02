using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.RegularExpressions;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers
{
    internal class FunctionApiDescriptionProvider : IApiDescriptionGroupCollectionProvider
    {
        private readonly ICompositeMetadataDetailsProvider compositeMetadataDetailsProvider;
        private readonly IModelMetadataProvider modelMetadataProvider;
        private readonly SwaggerDocOptions swaggerDocOptions;
        private readonly IOutputFormatter outputFormatter;

        public FunctionApiDescriptionProvider(
            IOptions<SwaggerDocOptions> functionsOptions,
            SwashBuckleStartupConfig startupConfig,
            IModelMetadataProvider modelMetadataProvider,
            ICompositeMetadataDetailsProvider compositeMetadataDetailsProvider,
            IOutputFormatter outputFormatter)
        {
            this.swaggerDocOptions = functionsOptions.Value;
            this.modelMetadataProvider = modelMetadataProvider;
            this.compositeMetadataDetailsProvider = compositeMetadataDetailsProvider;
            this.outputFormatter = outputFormatter;

            var apiDescGroups = new Dictionary<string, List<ApiDescription>>();
            var methods = startupConfig.Assembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(FunctionAttribute), false).Any())
                .ToArray();

            foreach (var methodInfo in methods)
            {
                if (!this.TryGetHttpTrigger(methodInfo, out var triggerAttribute))
                {
                    continue;
                }

                var functionAttr = (FunctionAttribute?)methodInfo.GetCustomAttribute(typeof(FunctionAttribute), false);

                if (functionAttr == null && triggerAttribute == null)
                {
                    continue;
                }

                var apiExplorerSettingsAttribute =
                    (ApiExplorerSettingsAttribute?)methodInfo.GetCustomAttribute(typeof(ApiExplorerSettingsAttribute), false)
                    ?? (ApiExplorerSettingsAttribute?)methodInfo.DeclaringType!.GetCustomAttribute(
                        typeof(ApiExplorerSettingsAttribute),
                        false);

                var prefix = string.IsNullOrWhiteSpace(functionsOptions.Value.RoutePrefix)
                    ? string.Empty
                    : $"{functionsOptions.Value.RoutePrefix.TrimEnd('/')}/";

                string? route = null;

                if (this.swaggerDocOptions.PrependOperationWithRoutePrefix)
                {
                    var routePart = !string.IsNullOrWhiteSpace(triggerAttribute!.Route)
                        ? triggerAttribute.Route
                        : functionAttr!.Name;

                    route = $"{prefix}{routePart}";
                }
                else
                {
                    route = !string.IsNullOrWhiteSpace(triggerAttribute!.Route)
                        ? triggerAttribute.Route
                        : functionAttr!.Name;
                }

                var routes = new List<(string Route, string RemoveParamName)>();

                var regex = new Regex("/\\{(?<paramName>\\w+)\\?\\}$");
                var match = regex.Match(route!);

                var routeParamRemoveRegex = new Regex(":[a-zA-Z]+(\\(.*\\))?");
                route = routeParamRemoveRegex.Replace(route!, string.Empty);

                if (match is { Success: true, Captures.Count: 1 })
                {
                    routes.Add(
                        (route.Replace(match.Value, string.Empty).Replace("//", "/"), match.Groups["paramName"].ToString()));
                    routes.Add((route.Replace(match.Value, match.Value.Replace("?", string.Empty)), string.Empty));
                }
                else
                {
                    routes.Add((route, string.Empty));
                }

                var verbs = triggerAttribute.Methods ?? ["get", "post", "delete", "head", "patch", "put", "options"];

                for (var index = 0; index < routes.Count; index++)
                {
                    var (routeString, removeParamName) = routes[index];
                    var apiName = functionAttr!.Name + (index == 0 ? string.Empty : $"-{index}");
                    var items = verbs.Select(verb => this.CreateDescription(
                            methodInfo,
                            routeString,
                            index,
                            functionAttr,
                            apiExplorerSettingsAttribute!,
                            verb,
                            triggerAttribute.AuthLevel,
                            removeParamName,
                            verbs.Length > 1)).ToArray();

                    var groupName =
                        (items.FirstOrDefault()?.ActionDescriptor as ControllerActionDescriptor)?.ControllerName ??
                        apiName;
                    if (!apiDescGroups.ContainsKey(groupName))
                    {
                        apiDescGroups[groupName] = new List<ApiDescription>();
                    }

                    apiDescGroups[groupName].AddRange(items);
                }
            }

            this.ApiDescriptionGroups = new ApiDescriptionGroupCollection(
                new ReadOnlyCollection<ApiDescriptionGroup>(
                    apiDescGroups.Select(
                        kv => new ApiDescriptionGroup(kv.Key, kv.Value)).ToList()),
                1);
        }

        public ApiDescriptionGroupCollection ApiDescriptionGroups { get; }

        private static void SetupDefaultJsonFormatterIfNone(IList<ApiRequestFormat> supportedMediaTypes)
        {
            if (supportedMediaTypes.Count == 0)
            {
                supportedMediaTypes.Add(new ApiRequestFormat
                {
                    MediaType = "application/json"
                });
            }
        }

        private static Regex GetRoutePathParamRegex(string parameterName)
        {
            return new Regex("\\{[" + parameterName + "]+[\\?]{0,1}\\}");
        }

        private bool TryGetHttpTrigger(MethodInfo methodInfo, out HttpTriggerAttribute? triggerAttribute)
        {
            triggerAttribute = null;
            var ignore = methodInfo.GetCustomAttributes().Any(x => x is SwaggerIgnoreAttribute);
            if (ignore)
            {
                return false;
            }

            triggerAttribute = this.FindHttpTriggerAttribute(methodInfo);

            if (triggerAttribute == null)
            {
                return false;
            }

            return true;
        }

        private ApiDescription CreateDescription(
            MethodInfo methodInfo,
            string route,
            int routeIndex,
            FunctionAttribute functionAttr,
            ApiExplorerSettingsAttribute apiExplorerSettingsAttr,
            string verb,
            AuthorizationLevel authorizationLevel,
            string? removeParamName = null,
            bool manyVerbs = false)
        {
            string controllerName;
            if (apiExplorerSettingsAttr?.GroupName != null)
            {
                controllerName = apiExplorerSettingsAttr.GroupName;
            }
            else
            {
                controllerName = methodInfo.DeclaringType!.Name.EndsWith("Controller")
                    ? methodInfo.DeclaringType.Name.Remove(
                        methodInfo.DeclaringType.Name.Length - "Controller".Length, "Controller".Length)
                    : functionAttr.Name;
            }

            var actionName = functionAttr.Name;
            var actionNamePrefix = (routeIndex > 0 ? $"_{routeIndex}" : string.Empty) + (manyVerbs ? $"_{verb.ToLower()}" : string.Empty);

            var description = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    MethodInfo = methodInfo,
                    ControllerName = controllerName,
                    DisplayName = actionName,
                    ControllerTypeInfo = methodInfo.DeclaringType!.GetTypeInfo(),
                    Parameters = new List<ParameterDescriptor>(),
                    RouteValues = new Dictionary<string, string?>
                    {
                        { "controller", controllerName },
                        { "action", actionName }
                    },
                    ActionName = !string.IsNullOrEmpty(actionNamePrefix) ? actionName + actionNamePrefix : string.Empty
                },
                RelativePath = route,
                HttpMethod = verb.ToUpper()
            };

            var supportedMediaTypes = methodInfo.GetCustomAttributes<SupportedRequestFormatAttribute>()
                .Select(x => new ApiRequestFormat { MediaType = x.MediaType })
                .ToList();

            SetupDefaultJsonFormatterIfNone(supportedMediaTypes);

            foreach (var supportedMediaType in supportedMediaTypes)
            {
                description.SupportedRequestFormats.Add(supportedMediaType);
            }

            var parameters = this.GetParametersDescription(methodInfo, route).ToList();

            foreach (var parameter in parameters.Where(parameter => parameter.Name != removeParamName))
            {
                description.ActionDescriptor.Parameters.Add(new ParameterDescriptor
                {
                    Name = parameter.Name,
                    ParameterType = parameter.Type
                });
                description.ParameterDescriptions.Add(parameter);
            }

            foreach (var apiResponseType in this.GetResponseTypes(methodInfo))
            {
                description.SupportedResponseTypes.Add(apiResponseType);
            }

            if (this.swaggerDocOptions.AddCodeParameter && authorizationLevel != AuthorizationLevel.Anonymous)
            {
                description.ParameterDescriptions.Add(new ApiParameterDescription
                {
                    Name = "code",
                    Type = typeof(string),
                    Source = BindingSource.Query,
                    RouteInfo = new ApiParameterRouteInfo
                    {
                        IsOptional = true
                    }
                });
            }

            return description;
        }

        private IEnumerable<ApiResponseType> GetResponseTypes(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(ProducesResponseTypeAttribute))
                .Select(customAttribute => customAttribute as ProducesResponseTypeAttribute)
                .Select(responseType =>
                {
                    var isVoidResponseType = responseType!.Type == typeof(void);

                    return new ApiResponseType
                    {
                        ApiResponseFormats = new[]
                        {
                            new ApiResponseFormat
                            {
                                Formatter = this.outputFormatter,
                                MediaType = "application/json"
                            }
                        },
                        ModelMetadata = isVoidResponseType ? null : this.modelMetadataProvider.GetMetadataForType(responseType.Type),
                        Type = isVoidResponseType ? null : responseType.Type,
                        StatusCode = responseType.StatusCode
                    };
                });
        }

        private HttpTriggerAttribute? FindHttpTriggerAttribute(MethodInfo methodInfo)
        {
            HttpTriggerAttribute? triggerAttribute = null;

            foreach (var parameter in methodInfo.GetParameters())
            {
                triggerAttribute = parameter.GetCustomAttributes(typeof(HttpTriggerAttribute), false)
                    .FirstOrDefault() as HttpTriggerAttribute;
                if (triggerAttribute != null)
                {
                    break;
                }
            }

            return triggerAttribute;
        }

        private IEnumerable<ApiParameterDescription> GetParametersDescription(MethodInfo methodInfo, string route)
        {
            foreach (var parameter in methodInfo.GetParameters())
            {
                var requestBodyTypeAttribute =
                    parameter.GetCustomAttribute(typeof(RequestBodyTypeAttribute)) as RequestBodyTypeAttribute;

                if ((parameter.ParameterType == typeof(HttpRequestMessage) ||
                     parameter.ParameterType == typeof(HttpRequest))
                    && requestBodyTypeAttribute == null)
                {
                    continue;
                }

                if (this.IgnoreParameter(parameter))
                {
                    continue;
                }

                var hasHttpTriggerAttribute = parameter.GetCustomAttributes()
                    .Any(attr => attr is HttpTriggerAttribute);

                var hasFromUriAttribute = false;

                var type = hasHttpTriggerAttribute && requestBodyTypeAttribute != null
                    ? requestBodyTypeAttribute.Type
                    : parameter.ParameterType;

                var regex = GetRoutePathParamRegex(parameter.Name!);
                var match = regex.Match(route);
                var bindingSource = match.Success ? BindingSource.Path
                    : hasFromUriAttribute ? BindingSource.Query
                    : BindingSource.Body;

                var optional = bindingSource == BindingSource.Query || match.Value.Contains("?");

                yield return new ApiParameterDescription
                {
                    Name = parameter.Name!,
                    Type = type,
                    Source = bindingSource,
                    RouteInfo = new ApiParameterRouteInfo
                    {
                        IsOptional = optional
                    },
                    ModelMetadata = new DefaultModelMetadata(
                        this.modelMetadataProvider,
                        this.compositeMetadataDetailsProvider,
                        new DefaultMetadataDetails(
                            ModelMetadataIdentity.ForType(type),
                            ModelAttributes.GetAttributesForType(type)))
                };
            }
        }

        private bool IgnoreParameter(ParameterInfo parameter)
        {
            var ignoreParameterAttribute = parameter.GetCustomAttribute(typeof(SwaggerIgnoreAttribute));
            if (ignoreParameterAttribute != null)
            {
                return true;
            }

            if (parameter.ParameterType.Name == "TraceWriter")
            {
                return true;
            }

            if (parameter.ParameterType == typeof(ExecutionContext))
            {
                return true;
            }

            if (parameter.ParameterType == typeof(ILogger))
            {
                return true;
            }

            if (parameter.ParameterType.IsAssignableFrom(typeof(ILogger)))
            {
                return true;
            }

            if (parameter.ParameterType.IsAssignableFrom(typeof(ISwashBuckleClient)))
            {
                return true;
            }

            if (parameter.GetCustomAttributes().Any(attr => attr is HttpTriggerAttribute)
                && parameter.GetCustomAttributes().All(attr => !(attr is RequestBodyTypeAttribute)))
            {
                return true;
            }

            return false;
        }
    }
}
