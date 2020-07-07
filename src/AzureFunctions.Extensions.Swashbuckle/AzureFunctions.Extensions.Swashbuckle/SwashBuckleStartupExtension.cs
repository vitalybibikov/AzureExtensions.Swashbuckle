using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Formatter;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;

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

            var formatter = new CustomJsonFormatter(new JsonSerializerOptions());
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
            //var x = Assembly.Load("Microsoft.AspNetCore.Mvc.Core, Version=3.1.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60")
            //    .GetTypes()
            //    .First(t => t.Name == "SystemTextJsonOutputFormatter");

            //var p = x
            //    .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            //    .Single(x => x.Name.Equals("CreateFormatter"))
            //    .Invoke(null, new[] { new JsonOptions(), });

            //var t = (SystemTextJsonOutputFormatter) p;
            builder.Services.AddSingleton<JsonSerializerOptions>(new JsonSerializerOptions());

            builder.Services.AddSingleton<SystemTextJsonOutputFormatter>(x =>
            {
                return new SystemTextJsonOutputFormatter(new JsonSerializerOptions());
            });

            builder.Services.AddSingleton<IModelMetadataProvider>(new EmptyModelMetadataProvider());
            builder.Services.AddSingleton(new SwashBuckleStartupConfig
            {
                Assembly = assembly
            });

            builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();

            return builder;
        }
    }
}