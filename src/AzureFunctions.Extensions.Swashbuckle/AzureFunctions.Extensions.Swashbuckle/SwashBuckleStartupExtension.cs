using System;
using System.Reflection;
using System.Text.Json;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleStartupExtension
    {
        [Obsolete("Please, use FunctionsStartup instead")]
        public static IWebJobsBuilder AddSwashBuckle(
            this IWebJobsBuilder builder,
            Assembly assembly,
            Action<SwaggerDocOptions> configureDocOptionsAction = null)
        {
            builder.AddExtension<SwashbuckleConfig>()
                .BindOptions<SwaggerDocOptions>()
                .ConfigureOptions<SwaggerDocOptions>((configuration, section, options) =>
                {
                    configureDocOptionsAction?.Invoke(options);
                });

            builder.Services.AddSingleton<IModelMetadataProvider>(new EmptyModelMetadataProvider());
            builder.Services.AddSingleton(new SwashBuckleStartupConfig
            {
                Assembly = assembly
            });

            var formatter = new SystemTextJsonOutputFormatter(new JsonSerializerOptions());
            builder.Services.AddSingleton<IOutputFormatter>(formatter);
            builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();

            return builder;
        }

        public static IFunctionsHostBuilder  AddSwashBuckle(
            this IFunctionsHostBuilder  builder,
            Assembly assembly,
            Action<SwaggerDocOptions> configureDocOptionsAction = null)
        {
            var wbBuilder = builder.Services.AddWebJobs(x => { return; });

            wbBuilder.AddExtension<SwashbuckleConfig>()
                .BindOptions<SwaggerDocOptions>()
                .ConfigureOptions<SwaggerDocOptions>((configuration, section, options) =>
                {
                    configureDocOptionsAction?.Invoke(options);
                });

            builder.Services.AddSingleton<IModelMetadataProvider>(new EmptyModelMetadataProvider());
            builder.Services.AddSingleton(new SwashBuckleStartupConfig
            {
                Assembly = assembly
            });

            var formatter = new SystemTextJsonOutputFormatter(new JsonSerializerOptions());
            builder.Services.AddSingleton<IOutputFormatter>(formatter);
            builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();

            return builder;
        }
    }
}