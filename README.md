
# AzureExtensions.Swashbuckle v4.0.0

### (Searching for collaborators!)

OpenAPI 2/3 implementation based on Swashbuckle(Swagger) tooling for API's built with Azure Functions 

This product aims to easily provide Swagger and Swagger UI of APIs created in Azure Functions using isolated worker model


------------------------------

4.0.3
by @MikeHookTransparity
Added ClientSecret, UseBasicAuthenticationWithAccessCodeGrant

------------------------------
4.0.2
- Updated to latest Swagger 6.5.1
- Fixes & Improvements
- Added symbols package

------------------------------
4.0.1

- Fixed some minor issues
- Updated to new language features
- Added exception handling and nullability
- Doocumenation fixes

------------------------------
4.0.0-beta
- just remebering what the heck is going om here
- Updated to v4 Functions
- Updated to .NET 8
- Updated to isolated worker model (from now on it is going to be the only one that is supported, as inprocess is going to be deprecated)
- Updated to UI v5.17.3
- Updated to Swagger 5.6.5
- Updated docs
- Considering removing support of NewtonJson

  https://www.nuget.org/packages/AzureExtensions.Swashbuckle/4.0.0-beta
------------------------------
3.3.1-beta

- #64 Support for authorization configuration
- #60 Consolidated extensions and added one to support .net 5
- Updated docs
- Updated js/html/css libs
- Some classed made public to support 3-party IoC.
- Fixed several issues, related to versioning and XML comments.
- Updated to UI v3.37.2
- Updated to Swagger 5.6.3
- Updated documentation
- Ability to create multiple versions of documents, example added.
- Added examples of a custom filter, improved test application

https://www.nuget.org/packages/AzureExtensions.Swashbuckle/4.0.0-beta


------------------------------
3.1.6

https://www.nuget.org/packages/AzureExtensions.Swashbuckle/3.1.6

Fixed #8, #9

Updated to UI v3.25.1

Updated to Swagger 5.4.1

Fixed base url for Swagger UI

**Breaking:**

Option and DocumentOption renamed to SwaggerDocOptions and SwaggerDocument respectivly
and moved to AzureFunctions.Extensions.Swashbuckle.Settings namespace

**Properties renamed:**

PrepandOperationWithRoutePrefix => PrependOperationWithRoutePrefix

AddCodeParamater => AddCodeParameter

**Properties added:**

Added ability to configure SwaggerGen via ConfigureSwaggerGen

Added ability to override default url to Swagger json document (in case of reverse proxy/gateway/ingress) are used.


**Size:**

All the resources are places in zip archive in order to decrease result dll size by 338% (from 1.594kb to 472kb)



------------------------------
3.0.0
- Updated to v3 Functions
- Updated to 5.0.0 Swashbuckle.AspNetCore nugets
- Merged PRs to fix issues related to RequestBodyType and Ignore attribute
- application/json is a default media type.



# Sample

https://github.com/vitalybibikov/azure-functions-extensions-swashbuckle/tree/master/src/AzureFunctions.Extensions.Swashbuckle/TestFunction

# Update

Version 3.0.0


# Getting Started

1. Install the standard Nuget package into your Azure Functions application.

```
Package Manager : Install-Package AzureExtensions.Swashbuckle
CLI : dotnet add package AzureExtensions.Swashbuckle
```

2. Add Program.cs class on your Functions project.

!!! Now you need to specify in option the RoutePrefix.

            opts.RoutePrefix = "api";


```csharp
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
            opts.OAuth2RedirectPath = "http://localhost:7071/api/swagger/oauth2-redirect";
        });
    })
    .Build();

host.Run();

```

3. Add swagger and swagger ui endpoint functions on your project.

```csharp
    public class SwaggerController
    {
        private readonly ISwashBuckleClient swashBuckleClient;

        public SwaggerController(ISwashBuckleClient swashBuckleClient)
        {
            this.swashBuckleClient = swashBuckleClient;
        }

        [SwaggerIgnore]
        [Function("SwaggerJson")]
        public async Task<HttpResponseData> SwaggerJson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/json")]
            HttpRequestData  req)
        {
            return await this.swashBuckleClient.CreateSwaggerJsonDocumentResponse(req);
        }

        [SwaggerIgnore]
        [Function("SwaggerYaml")]
        public async Task<HttpResponseData> SwaggerYaml(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/yaml")]
            HttpRequestData req)
        {
            return await this.swashBuckleClient.CreateSwaggerYamlDocumentResponse(req);
        }

        [SwaggerIgnore]
        [Function("SwaggerUi")]
        public async Task<HttpResponseData> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/ui")]
            HttpRequestData req)
        {
            return await this.swashBuckleClient.CreateSwaggerUIResponse(req, "swagger/json");
        }

        /// <summary>
        /// This is only needed for OAuth2 client. This redirecting document is normally served
        /// as a static content. Functions don't provide this out of the box, so we serve it here.
        /// Don't forget to set OAuth2RedirectPath configuration option to reflect this route.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="swashBuckleClient"></param>
        /// <returns></returns>
        [SwaggerIgnore]
        [Function("SwaggerOAuth2Redirect")]
        public async Task<HttpResponseData> SwaggerOAuth2Redirect(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "swagger/oauth2-redirect")]
            HttpRequestData req)
        {
            return await this.swashBuckleClient.CreateSwaggerOAuth2RedirectResponse(req);
        }
    }
```

4. Open Swagger UI URL in your browser.

If you does not changed api route prefix. Swagger UI URL is https://hostname/api/swagger/ui .

## Options

### Include Xml document file

AzureFunctions.Extensions.Swashbuckle can include xml document file.

1. Change your functions project's GenerateDocumentationFile option to enable.

            builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), opts =>
            {
                opts.XmlPath = "TestFunction.xml";
            });

2. Add configration setting this extensions on your functions project's local.settings.json

```json
  "SwaggerDocOptions": {
    "XmlPath": "TestFunction.xml"
  }
```

Alternatively you can add this section to your host.json

```json
  "extensions": {
    "Swashbuckle": {
      "XmlPath": "TestFunction.xml"
    }
  }
```
