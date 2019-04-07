# azure-functions-extensions-swashbuckle

Swagger tooling for API's built with Azure Functions. 

This product aims to easily provide Swagger and Swagger UI of APIs created in Azure Functions using Swashbuckle.AspNetCore.

# Getting Started

1. Install the standard Nuget package into your Azure Functions application.

```
Package Manager : Install-Package AzureFunctions.Extensions.Swashbuckle
CLI : dotnet add package AzureFunctions.Extensions.Swashbuckle
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
