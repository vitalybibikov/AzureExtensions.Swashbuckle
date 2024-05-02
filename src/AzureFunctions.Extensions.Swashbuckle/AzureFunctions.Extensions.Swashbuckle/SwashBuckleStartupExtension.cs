using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleStartupExtension
    {
        public static IServiceCollection AddSwashBuckle(
            this IServiceCollection services,
            Action<SwaggerDocOptions>? configureDocOptionsAction = null,
            Assembly? executingAssembly = null)
        {
            if (configureDocOptionsAction != null)
            {
                services.Configure(configureDocOptionsAction);
            }

            Assembly? assembly;
            
            if (executingAssembly == null)
            {
                assembly = Assembly.GetEntryAssembly();

                if (assembly == null)
                {
                    throw new ArgumentNullException(nameof(assembly));
                }
            }
            else
            {
                assembly = executingAssembly;
            }

            services.AddSingleton<ISwashBuckleClient, SwashBuckleClient>();
            services.AddSingleton<SwashbuckleConfig>();
            services.AddSingleton<IModelMetadataProvider>(new EmptyModelMetadataProvider());
            services.AddSingleton(new SwashBuckleStartupConfig
            {
                Assembly = assembly
            });

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            var formatter = new SystemTextJsonOutputFormatter(jsonOptions);
            services.AddSingleton<IOutputFormatter>(formatter);
            services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();

            return services;
        }
    }
}
