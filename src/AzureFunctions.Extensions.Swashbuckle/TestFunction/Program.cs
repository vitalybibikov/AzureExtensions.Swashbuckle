using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using AzureFunctions.Extensions.Swashbuckle;
using Swashbuckle.AspNetCore.SwaggerGen;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
    })
    .ConfigureAppConfiguration((hostContext, services) =>
    {
       
    })
    .ConfigureServices((hostContext, services) =>
    {
        //Register the extension
        services.AddSwashBuckle(opts =>
        {
            // If you want to add Newtonsoft support insert next line
            // opts.AddNewtonsoftSupport = true;
            opts.RoutePrefix = "api";
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
                x.AddSecurityDefinition("oauth2",
                    new OpenApiSecurityScheme
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
            opts.ClientSecret = "your.client.secret";
            opts.UsePkceWithAuthorizationCodeGrant = true;
            opts.OAuth2RedirectPath = "http://localhost:7071/api/swagger/oauth2-redirect";
        });
    })
    .Build();

host.Run();
