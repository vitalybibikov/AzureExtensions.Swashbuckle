using System;
using System.Reflection;
using System.Text.Json;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

            services.TryAdd(ServiceDescriptor.Transient<ICompositeMetadataDetailsProvider>(s =>
            {
                var options = s.GetRequiredService<IOptions<MvcOptions>>().Value;
                return new DefaultCompositeMetadataDetailsProvider(options.ModelMetadataDetailsProviders);
            }));
            services.AddSingleton(new SwashBuckleStartupConfig
            {
                Assembly = assembly
            });

            var swaggerDocOptions = new SwaggerDocOptions();
            configureDocOptionsAction?.Invoke(swaggerDocOptions);

            services.AddSingleton(swaggerDocOptions);
            
            var formatter = new SystemTextJsonOutputFormatter(new JsonSerializerOptions());
            services.AddSingleton<IOutputFormatter>(formatter);
            services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();
            services.AddSingleton<ISwashbuckleConfig, SwashbuckleConfig>();
            services.AddSingleton<ISwashBuckleClient, SwashBuckleClient>();

            services.InitializeSwashbuckleConfig();

            return services;
        }

        private static void InitializeSwashbuckleConfig(this IServiceCollection services)
        {
            var providers = services.BuildServiceProvider();
            var config = providers.GetRequiredService<ISwashbuckleConfig>();
            var description = providers.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            config.Initialize(description);
        }
    }
}
