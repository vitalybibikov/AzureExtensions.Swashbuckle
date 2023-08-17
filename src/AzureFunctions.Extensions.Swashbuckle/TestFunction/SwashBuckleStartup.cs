using System;
using System.Collections.Generic;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TestFunction;

[assembly: FunctionsStartup(typeof(SwashBuckleStartup))]
namespace TestFunction
{
    internal class SwashBuckleStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), swaggerDocOptions =>
            {
                swaggerDocOptions.SpecVersion = OpenApiSpecVersion.OpenApi3_0;
                swaggerDocOptions.AddCodeParameter = true;
                swaggerDocOptions.PrependOperationWithRoutePrefix = true;
                swaggerDocOptions.XmlPath = "TestFunction.xml";
                swaggerDocOptions.Documents = new[]
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
                swaggerDocOptions.Title = "Swagger Test";
                //opts.OverridenPathToSwaggerJson = new Uri("http://localhost:7071/api/Swagger/json");
                swaggerDocOptions.ConfigureSwaggerGen = swaggerGenOptions =>
                {
                    //custom operation example
                    swaggerGenOptions.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)
                        ? methodInfo.Name
                        : new Guid().ToString());

                    // enable polymorphism for models
                    swaggerGenOptions.UseOneOfForPolymorphism();
                    // let swagger know what the discriminator property's name is
                    swaggerGenOptions.SelectDiscriminatorNameUsing(_ => "$type");

                    //custom filter example
                    //x.DocumentFilter<RemoveSchemasFilter>();

                    //oauth2
                    swaggerGenOptions.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
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
                swaggerDocOptions.ClientId = "your.client.id";
                swaggerDocOptions.OAuth2RedirectPath = "http://localhost:7071/api/swagger/oauth2-redirect";

            });
        }
    }
}
