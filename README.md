# AzureExtensions.Swashbuckle

Swagger and Swagger UI for **Azure Functions** (isolated worker model) powered by Swashbuckle. Supports **OpenAPI 2.0, 3.0, and 3.1**.

<p align="center">

[![CI](https://github.com/vitalybibikov/AzureExtensions.Swashbuckle/actions/workflows/ci.yml/badge.svg)](https://github.com/vitalybibikov/AzureExtensions.Swashbuckle/actions/workflows/ci.yml)
[![Auto-Release](https://github.com/vitalybibikov/AzureExtensions.Swashbuckle/actions/workflows/auto-release.yml/badge.svg)](https://github.com/vitalybibikov/AzureExtensions.Swashbuckle/actions/workflows/auto-release.yml)
[![NuGet](https://img.shields.io/nuget/v/AzureExtensions.Swashbuckle?logo=nuget&label=NuGet)](https://www.nuget.org/packages/AzureExtensions.Swashbuckle)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AzureExtensions.Swashbuckle?logo=nuget&label=Downloads)](https://www.nuget.org/packages/AzureExtensions.Swashbuckle)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

</p>

<p align="center">

![.NET 8](https://img.shields.io/badge/.NET-8.0_LTS-512BD4?logo=dotnet&logoColor=white)
![.NET 9](https://img.shields.io/badge/.NET-9.0_STS-512BD4?logo=dotnet&logoColor=white)
![Swashbuckle 10](https://img.shields.io/badge/Swashbuckle-10.x-85EA2D?logo=swagger&logoColor=white)
![Swagger UI](https://img.shields.io/badge/Swagger_UI-5.32.0-85EA2D?logo=swagger&logoColor=white)
![OpenAPI](https://img.shields.io/badge/OpenAPI-2.0_|_3.0_|_3.1-6BA539?logo=openapiinitiative&logoColor=white)
![Azure Functions](https://img.shields.io/badge/Azure_Functions-Isolated_Worker-0062AD?logo=azurefunctions&logoColor=white)
![Tests](https://img.shields.io/badge/Tests-136_passed-brightgreen?logo=dotnet&logoColor=white)

</p>

---

## What's New in v5.0.1

- Updated embedded Swagger UI from v5.11 to **v5.32.0**
- Fixed resource leaks in response extension methods
- Fixed route parameter regex matching bug
- Fixed `FunctionContext` incorrectly treated as body parameter ([#122](https://github.com/vitalybibikov/AzureExtensions.Swashbuckle/issues/122))
- Fixed XML documentation file not found in TFM subdirectories ([#114](https://github.com/vitalybibikov/AzureExtensions.Swashbuckle/issues/114))
- Fixed Content-Type header on JSON/YAML responses ([#119](https://github.com/vitalybibikov/AzureExtensions.Swashbuckle/issues/119))
- Added `IDisposable` to `SwashbuckleConfig` for proper cleanup
- Added 136 unit, integration, and end-to-end tests

## Features

- **Isolated Worker Model** — Built for Azure Functions v4 isolated worker (the recommended model going forward)
- **Swashbuckle 10.x** — Latest Swashbuckle.AspNetCore with Microsoft.OpenApi v2 support
- **OpenAPI 2.0 / 3.0 / 3.1** — Generate specs in any supported version
- **Swagger UI** — Embedded Swagger UI served directly from your Azure Function
- **Multi-document** — Define multiple API versions/documents
- **XML Comments** — Automatic parameter and response documentation from XML docs
- **Custom Attributes** — `[QueryStringParameter]`, `[RequestHttpHeader]`, `[SwaggerUploadFile]`, `[RequestBodyType]`
- **OAuth2 Support** — Built-in OAuth2 redirect endpoint and client configuration
- **Newtonsoft.Json** — Optional Newtonsoft serialization support
- **Multi-targeting** — .NET 8.0 (LTS) and .NET 9.0 (STS)

---

## Installation

```bash
dotnet add package AzureExtensions.Swashbuckle
```

## Quick Start

### 1. Register the Extension in `Program.cs`

```csharp
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

var host = new HostBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSwashBuckle(opts =>
        {
            opts.RoutePrefix = "api";
            opts.SpecVersion = OpenApiSpecVersion.OpenApi3_0;
            opts.AddCodeParameter = true;
            opts.PrependOperationWithRoutePrefix = true;
            opts.XmlPath = "MyFunctionApp.xml";
            opts.Documents = new[]
            {
                new SwaggerDocument
                {
                    Name = "v1",
                    Title = "My API",
                    Description = "My Azure Functions API",
                    Version = "v1"
                }
            };
            opts.Title = "My API";
        });
    })
    .Build();

host.Run();
```

### 2. Add Swagger Endpoints

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
        HttpRequestData req)
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
}
```

### 3. Open Swagger UI

Navigate to `https://your-function-app/api/swagger/ui` in your browser.

---

## Configuration Options

### XML Documentation

Enable XML doc generation in your `.csproj`:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Then pass the XML path:

```csharp
opts.XmlPath = "MyFunctionApp.xml";
```

### Multiple Documents

```csharp
opts.Documents = new[]
{
    new SwaggerDocument { Name = "v1", Title = "API v1", Version = "v1" },
    new SwaggerDocument { Name = "v2", Title = "API v2", Version = "v2" }
};
```

### OAuth2

```csharp
opts.ConfigureSwaggerGen = x =>
{
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

opts.ClientId = "your.client.id";
opts.OAuth2RedirectPath = "http://localhost:7071/api/swagger/oauth2-redirect";
```

### Custom Attributes

```csharp
// Add query string parameters
[QueryStringParameter("page", "Page number", DataType = typeof(int), Required = false)]
[Function("GetItems")]
public async Task<HttpResponseData> GetItems(...)

// Add required HTTP headers
[RequestHttpHeader("X-Api-Key", isRequired: true)]
[Function("SecureEndpoint")]
public async Task<HttpResponseData> SecureEndpoint(...)

// File upload
[SwaggerUploadFile("file", "File to upload")]
[Function("Upload")]
public async Task<HttpResponseData> Upload(...)
```

### Newtonsoft.Json Support

```csharp
services.AddSwashBuckle(opts =>
{
    opts.AddNewtonsoftSupport = true;
    // ...
});
```

---

## Migration from v4.x to v5.x

v5.0 includes breaking changes due to the Swashbuckle 10.x / Microsoft.OpenApi v2 upgrade:

| Change | v4.x | v5.x |
|--------|------|------|
| Swagger document methods | Synchronous | **Async** (`GetSwaggerJsonDocumentAsync`) |
| OpenAPI schema types | `Type = "string"` | `Type = JsonSchemaType.String` |
| Nullable schemas | `Nullable = true` | `JsonSchemaType.X \| JsonSchemaType.Null` |
| OpenAPI namespace | `Microsoft.OpenApi.Models` | `Microsoft.OpenApi` |

### Async Document Methods

```csharp
// v4.x
Stream json = client.GetSwaggerJsonDocument(req, "v1");

// v5.x
Stream json = await client.GetSwaggerJsonDocumentAsync(req, "v1");
```

---

## Sample Project

See the [TestFunction](https://github.com/vitalybibikov/AzureExtensions.Swashbuckle/tree/master/src/AzureFunctions.Extensions.Swashbuckle/TestFunction) project for a complete working example.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

Copyright &copy; 2026, Vitali Bibikov. Code released under the [MIT License](LICENSE).
