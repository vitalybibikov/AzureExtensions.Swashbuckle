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
        [Obsolete("Please, use FunctionsStartup instead, will be deprecated soon")]
        public static IWebJobsBuilder AddSwashBuckle(
            this IWebJobsBuilder builder,
            Assembly assembly,
            Action<SwaggerDocOptions> configureDocOptionsAction = null)
        {
            builder.Services.AddSwashBuckle(assembly, configureDocOptionsAction, builder);
            return builder;
        }

        public static IFunctionsHostBuilder  AddSwashBuckle(
            this IFunctionsHostBuilder  builder,
            Assembly assembly,
            Action<SwaggerDocOptions> configureDocOptionsAction = null)
        {
            builder.Services.AddSwashBuckle(assembly, configureDocOptionsAction);
            return builder;
        }

        public static IServiceCollection AddSwashBuckle(
            this IServiceCollection services,
            Assembly assembly,
            Action<SwaggerDocOptions> configureDocOptionsAction = null, 
            IWebJobsBuilder webJobsBuilder = null)
        {
            webJobsBuilder ??= services.AddWebJobs(_ => { });

            webJobsBuilder.AddExtension<SwashbuckleConfig>()
                .BindOptions<SwaggerDocOptions>()
                .ConfigureOptions<SwaggerDocOptions>((configuration, section, options) => configureDocOptionsAction?.Invoke(options));

            services.AddSingleton<IModelMetadataProvider>(new EmptyModelMetadataProvider());
            services.AddSingleton(new SwashBuckleStartupConfig
            {
                Assembly = assembly
            });

            var formatter = new SystemTextJsonOutputFormatter(new JsonSerializerOptions());
            services.AddSingleton<IOutputFormatter>(formatter);
            services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();

            return services;
        }
    }
}