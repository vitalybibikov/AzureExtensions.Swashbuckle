using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Synigo.OneApi.Core.Functions.Extensions;
using System;
using AzureFunctions.Extensions.Swashbuckle;
using System.Reflection;
using Microsoft.OpenApi;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace TestFunction
{
    public static class Program
    {

        public static void Main(string[] args)
            => CreateHostBuilder(args)
            .Build()
            .Run();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddDebug();
                    logging.AddConsole();
                })
                .ConfigureFunctionsWorkerDefaults(workerApplication =>
                {
                    //exception middleware form synigo.openapi.core.functions
                    workerApplication.UseOneApiExceptionMiddleware();
                })
                .ConfigureServices(service => ConfigureServices(service))
                .AddSwashBuckle(Assembly.GetExecutingAssembly(), opts =>
                {
                opts.SpecVersion = OpenApiSpecVersion.OpenApi3_0;
                opts.AddCodeParameter = true;
                opts.PrependOperationWithRoutePrefix = true;
                opts.XmlPath = "TestFunction.xml";
                opts.Documents = new[]
                {
                    new SwaggerDocument
                    {
                        Name = "v1",
                        Title = "Swagger document",
                        Description = "Swagger test document",
                        Version = "v2"
                    },
                    new SwaggerDocument
                    {
                        Name = "v2",
                        Title = "Swagger document 2",
                        Description = "Swagger test document 2",
                        Version = "v2"
                    }
                };
                opts.Title = "Swagger Test";
                //opts.OverridenPathToSwaggerJson = new Uri("http://localhost:7071/api/Swagger/json");
                opts.ConfigureSwaggerGen = x =>
                {
                    //custom operation example
                    x.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)
                        ? methodInfo.Name
                        : new Guid().ToString());

                    //custom filter example
                    //x.DocumentFilter<RemoveSchemasFilter>();

                    //oauth2
                    x.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://your.idserver.net/connect/authorize"),
                                Scopes = new Dictionary<string, string>
                                {
                                    { "api.read", "Access read operations" },
                                    { "api.write", "Access write operations" }
                                }
                            }
                        }
                    });
                };

                // set up your client ID if your API is protected
                opts.ClientId = "your.client.id";
                opts.OAuth2RedirectPath = "http://localhost:7071/api/swagger/oauth2-redirect";

            });

            return hostBuilder;
        }
        internal static void ConfigureServices(IServiceCollection services)
        {
            services.UseOneApiGenerateHttpResponseData();
        }
    }
}
