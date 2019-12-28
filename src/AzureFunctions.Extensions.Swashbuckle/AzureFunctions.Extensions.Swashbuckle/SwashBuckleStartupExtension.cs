using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.Extensions.Swashbuckle
{
    public static class SwashBuckleStartupExtension
    {
        public static IWebJobsBuilder AddSwashBuckle(this IWebJobsBuilder builder, Assembly assembly)
        {
            builder.AddExtension<SwashbuckleConfig>()
                .BindOptions<Option>()
                .Services.AddSingleton(new SwashBuckleStartupConfig
                {
                    Assembly = assembly
                })
                ;
            builder.Services.AddSingleton<IOutputFormatter>(c =>
                new SystemTextJsonOutputFormatter(new JsonSerializerOptions()
                {
                }));

            builder.Services.AddSingleton<IModelMetadataProvider>(c => new EmptyModelMetadataProvider());

            builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();

            return builder;
        }
    }
}
