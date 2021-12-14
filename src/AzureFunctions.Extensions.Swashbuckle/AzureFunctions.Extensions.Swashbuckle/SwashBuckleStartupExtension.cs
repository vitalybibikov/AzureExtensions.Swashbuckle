using System;
using System.Reflection;
using System.Text.Json;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleStartupExtension
    {
      
        public static IHostBuilder  AddSwashBuckle(
            this IHostBuilder builder,
            Assembly assembly,
            Action<SwaggerDocOptions> configureDocOptionsAction = null)
        {
            builder.ConfigureServices((services) =>
            {
                services.AddSwashBuckle(assembly, configureDocOptionsAction);
            });
            
            return builder;
        }

        public static IServiceCollection AddSwashBuckle(
            this IServiceCollection services,
            Assembly assembly,
            Action<SwaggerDocOptions> configureDocOptionsAction = null)
        {

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