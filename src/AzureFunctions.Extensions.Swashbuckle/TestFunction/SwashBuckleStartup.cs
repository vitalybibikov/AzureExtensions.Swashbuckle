using System;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using TestFunction;

[assembly: WebJobsStartup(typeof(SwashBuckleStartup))]

namespace TestFunction
{
    internal class SwashBuckleStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), opts =>
            {
                opts.SpecVersion = OpenApiSpecVersion.OpenApi3_0;
                opts.AddCodeParameter = true;
                opts.PrependOperationWithRoutePrefix = false;
                opts.Documents = new []
                {
                    new SwaggerDocument
                    {
                        Name = "v1",
                        Title = "Swagger document",
                        Description = "Swagger test document",
                        Version = "v2"
                    }
                };
                opts.Title = "Swagger Test";
                opts.ConfigureSwaggerGen = (x =>
                {
                    x.CustomOperationIds(apiDesc =>
                    {
                        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)
                            ? methodInfo.Name
                            : new Guid().ToString();
                    });
                });
            });
        }
    }
}