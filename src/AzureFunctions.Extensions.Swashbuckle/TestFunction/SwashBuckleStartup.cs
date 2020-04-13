using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.OpenApi;
using TestFunction;

[assembly: WebJobsStartup(typeof(SwashBuckleStartup))]

namespace TestFunction
{
    internal class SwashBuckleStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), new SwaggerDocOptions()
            {
                Title = "Swagger Test",
                AddCodeParameter = true,
                PrependOperationWithRoutePrefix = false,
                SpecVersion = OpenApiSpecVersion.OpenApi3_0,
                Documents = new []
                {
                    new SwaggerDocument()
                    {
                        Name = "v1",
                        Title = "Swagger document",
                        Description = "Swagger test document",
                        Version = "v2"
                    }
                }
            });
        }
    }
}