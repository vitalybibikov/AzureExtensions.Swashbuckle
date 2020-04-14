
# azure-functions-extensions-swashbuckle

Swagger tooling for API's built with Azure Functions. 

This product aims to easily provide Swagger and Swagger UI of APIs created in Azure Functions using Swashbuckle.AspNetCore.

------------------------------
3.1.2

https://www.nuget.org/packages/AzureExtensions.Swashbuckle/3.1.1-beta

**Breaking:**

Option and DocumentOption renamed to SwaggerDocOptions and SwaggerDocument respectivly
and moved to AzureFunctions.Extensions.Swashbuckle.Settings namespace

**Properties renamed:**

PrepandOperationWithRoutePrefix => PrependOperationWithRoutePrefix

AddCodeParamater => AddCodeParameter

**Properties added:**
Added ability to configure SwaggerGen via ConfigureSwaggerGen
Added ability to override default url to Swagger json document (in case of reverse proxy/gateway/ingress) are used.

**Versions:**

Updated to Swagger 5.3.1

Updated UI to v3.24.0 version to support both 2/3 OpenAPI versions correctly

**Size:**

All the resources are places in zip archive in order to decrease result dll size by 338% (from 1.594kb to 472kb)



------------------------------
3.0.0
- Updated to v3 Functions
- Updated to 5.0.0 Swashbuckle.AspNetCore nugets
- Merged PRs to fix issues related to RequestBodyType and Ignore attribute
- application/json is a default media type.



# Sample

https://github.com/vitalybibikov/azure-functions-extensions-swashbuckle/tree/master/sample

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

2. Add configration setting this extensions on your functions project's hots.json

```json
{
  "version": "2.0",
  "extensions": {
    "Swashbuckle": {
      "XmlPath":  "{your document xml file name}" 
    } 
  }
}
```
