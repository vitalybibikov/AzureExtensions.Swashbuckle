
# AzureExtensions.Swashbuckle

Swagger tooling for API's built with Azure Functions. 

This product aims to easily provide Swagger and Swagger UI of APIs created in Azure Functions using Swashbuckle.AspNetCore.


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

2. Add startup class on your Functions project.
```csharp
[assembly: WebJobsStartup(typeof(SwashBuckleStartup))]
namespace YourAppNamespace
{
    internal class SwashBuckleStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());

        }
    }
}
```

or you can create a more defailed configuration like this:

```
        public void Configure(IWebJobsBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), opts =>
            {
                opts.SpecVersion = OpenApiSpecVersion.OpenApi2_0;
                opts.AddCodeParameter = true;
                opts.PrependOperationWithRoutePrefix = true;
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
                //opts.OverridenPathToSwaggerJson = new Uri("http://localhost:7071/api/Swagger/json");
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
```

3. Add swagger and swagger ui endpoint functions on your project.

```csharp
public static class SwaggerController
{
    [SwaggerIgnore]
    [FunctionName("Swagger")]
    public static Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/json")] HttpRequestMessage req,
        [SwashBuckleClient]ISwashBuckleClient swashBuckleClient)
    {
        return Task.FromResult(swashBuckleClient.CreateSwaggerDocumentResponse(req));
    }

    [SwaggerIgnore]
    [FunctionName("SwaggerUi")]
    public static Task<HttpResponseMessage> Run2(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Swagger/ui")] HttpRequestMessage req,
        [SwashBuckleClient]ISwashBuckleClient swashBuckleClient)
    {
        return Task.FromResult(swashBuckleClient.CreateSwaggerUIResponse(req, "swagger/json"));
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
