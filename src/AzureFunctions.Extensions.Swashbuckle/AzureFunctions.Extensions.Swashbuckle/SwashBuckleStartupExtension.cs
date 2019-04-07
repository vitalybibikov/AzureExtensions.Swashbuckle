using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
                new JsonOutputFormatter(new JsonSerializerSettings(), ArrayPool<char>.Create()));

            builder.Services.AddSingleton<IModelMetadataProvider>(c => new DefaultModelMetadataProvider(
                new DefaultCompositeMetadataDetailsProvider(
                new List<IMetadataDetailsProvider>()
                {
                    new DefaultValidationMetadataProvider(),
                })));

            builder.Services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();

            return builder;
        }
    }
}
