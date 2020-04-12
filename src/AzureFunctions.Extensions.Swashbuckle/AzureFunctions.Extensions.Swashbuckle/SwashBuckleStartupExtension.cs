using System.Reflection;
using System.Text.Json;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Providers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleStartupExtension
    {
        public static IWebJobsBuilder AddSwashBuckle(
            this IWebJobsBuilder builder,
            Assembly assembly,
            SwaggerDocOptions docOptions = null)
        {
            builder.AddExtension<SwashbuckleConfig>()
                .Services.AddSingleton(new SwashBuckleStartupConfig
                {
                    Assembly = assembly
                });

            var formatter = new SystemTextJsonOutputFormatter(new JsonSerializerOptions());

            builder.Services.AddSingleton<IModelMetadataProvider>(new EmptyModelMetadataProvider());
            builder.Services.AddSingleton<IOutputFormatter>(formatter);
            builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();


            if (docOptions == null)
            {
                docOptions = new SwaggerDocOptions();
            }

            builder.Services.AddSingleton<SwaggerDocOptions>(docOptions);

            return builder;
        }
    }
}