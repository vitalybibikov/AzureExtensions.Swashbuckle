using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleStartupExtension
    {
        public static IFunctionsWorkerApplicationBuilder AddSwashBuckle(
            this IFunctionsWorkerApplicationBuilder builder,
            Assembly assembly,
            Action<SwaggerDocOptions> configureDocOptionsAction = null)
        {
            builder.Services.AddSwashBuckle(configureDocOptionsAction);
            return builder;
        }

        public static IServiceCollection AddSwashBuckle(
            this IServiceCollection services,
            Action<SwaggerDocOptions> configureDocOptionsAction = null)
        {
            services.Configure(configureDocOptionsAction);

            services.AddSingleton<ISwashBuckleClient, SwashBuckleClient>();
            services.AddSingleton<SwashbuckleConfig>();
            services.AddSingleton<IModelMetadataProvider>(new EmptyModelMetadataProvider());
            services.AddSingleton(new SwashBuckleStartupConfig
            {
                Assembly = Assembly.GetEntryAssembly()
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
