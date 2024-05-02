using System.IO.Compression;
using System.Reflection;
using System.Xml.XPath;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Extensions;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle
{
    public sealed class SwashbuckleConfig
    {
        private const string ZippedResources = "EmbededResources.resources.zip";
        private const string IndexHtmlName = "index.html";
        private const string SwaggerUiName = "swagger-ui.css";
        private const string SwaggerUiJsName = "swagger-ui-bundle.js";
        private const string SwaggerUiJsPresetName = "swagger-ui-standalone-preset.js";
        private const string SwaggerOAuth2RedirectName = "oauth2-redirect.html";

        private static readonly Lazy<string> IndexHtml = new Lazy<string>(() =>
        {
            var indexHtml = string.Empty;
            var assembly = GetAssembly();

            using var stream = assembly.GetResourceByName(ZippedResources);
            if (stream != null)
            {
                using var archive = new ZipArchive(stream);

                indexHtml = LoadAndUpdateDocument(indexHtml, archive, IndexHtmlName);
                indexHtml = LoadAndUpdateDocument(indexHtml, archive, SwaggerUiName, "{style}");
                indexHtml = LoadAndUpdateDocument(indexHtml, archive, SwaggerUiJsName, "{bundle.js}");
                indexHtml = LoadAndUpdateDocument(indexHtml, archive, SwaggerUiJsPresetName, "{standalone-preset.js}");
            }
            else
            {
                throw new ArgumentNullException(nameof(stream), "Embedded data stream is null");
            }

            return indexHtml;
        });

        private readonly IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider;

        private readonly Lazy<string> indexHtmlLazy;
        private readonly Lazy<string> oauth2RedirectLazy;
        private readonly SwaggerDocOptions swaggerOptions;
        private readonly string? xmlPath;
        private ServiceProvider? serviceProvider;

        public SwashbuckleConfig(
            IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            IOptions<SwaggerDocOptions> swaggerDocOptions,
            SwashBuckleStartupConfig startupConfig)
        {
            this.apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
            this.swaggerOptions = swaggerDocOptions.Value;

            if (!string.IsNullOrWhiteSpace(this.swaggerOptions.XmlPath))
            {
                var binPath = Path.GetDirectoryName(startupConfig.Assembly.Location);

                if (string.IsNullOrEmpty(binPath))
                {
                    throw new ArgumentNullException(nameof(binPath), "Not found path to an xml directory");
                }

                var binDirectory = Directory.CreateDirectory(binPath);
                var xmlBasePath = binDirectory?.Parent?.FullName;

                if (xmlBasePath != null)
                {
                    var path = Path.Combine(xmlBasePath, this.swaggerOptions.XmlPath);

                    if (File.Exists(path))
                    {
                        this.xmlPath = path;
                    }
                }
            }

            this.indexHtmlLazy = new Lazy<string>(
                () => IndexHtml.Value.Replace("{title}", this.swaggerOptions.Title));

            this.oauth2RedirectLazy = new Lazy<string>(() =>
            {
                var assembly = GetAssembly();
                using var stream = assembly.GetResourceByName(ZippedResources);
                using var archive = new ZipArchive(stream!);
                var entry = archive.GetEntry(SwaggerOAuth2RedirectName);
                using var entryStream = entry!.Open();
                using var reader = new StreamReader(entryStream);
                return reader.ReadToEnd();
            });

            this.Initialize();
        }

        public string RoutePrefix => this.swaggerOptions.RoutePrefix;

        public void Initialize()
        {
            var services = new ServiceCollection();

            services.AddSingleton(this.apiDescriptionGroupCollectionProvider);

            services.AddSingleton<IWebHostEnvironment>(new FunctionHostingEnvironment());

            if (this.swaggerOptions.AddNewtonsoftSupport)
            {
                services.AddSwaggerGenNewtonsoftSupport();
            }

            services.AddSwaggerGen(options =>
            {
                if (!this.swaggerOptions.Documents.Any())
                {
                    var defaultDocument = new SwaggerDocument();
                    AddSwaggerDocument(options, defaultDocument);
                }
                else
                {
                    foreach (var optionDocument in this.swaggerOptions.Documents)
                    {
                        AddSwaggerDocument(options, optionDocument);
                    }
                }

                if (!string.IsNullOrWhiteSpace(this.xmlPath))
                {
                    var xmlDoc = new XPathDocument(this.xmlPath);
                    options.IncludeXmlComments(this.xmlPath);
                    options.OperationFilter<XmlCommentsOperationFilterWithParams>(xmlDoc);
                    options.ParameterFilter<XmlCommentsParameterFilterWithExamples>(xmlDoc);
                    options.SchemaFilter<XmlCommentsSchemaFilterChanged>(xmlDoc);
                }

                options.OperationFilter<FunctionsOperationFilter>();
                options.OperationFilter<QueryStringParameterAttributeFilter>();
                options.OperationFilter<GenerateOperationIdFilter>();
                options.OperationFilter<FileUploadOperationFilter>();

                this.swaggerOptions.ConfigureSwaggerGen?.Invoke(options);
            });

            this.serviceProvider = services.BuildServiceProvider(true);
        }

        public string GetSwaggerOAuth2RedirectContent()
        {
            return this.oauth2RedirectLazy.Value;
        }

        public string GetSwaggerUIContent(string swaggerUrl)
        {
            if (this.swaggerOptions.OverridenPathToSwaggerJson != null)
            {
                swaggerUrl = this.swaggerOptions.OverridenPathToSwaggerJson.ToString();
            }

            var html = this.indexHtmlLazy.Value;
            return html
                .Replace("{url}", swaggerUrl)
                .Replace("{oauth2RedirectUrl}", this.swaggerOptions.OAuth2RedirectPath)
                .Replace("{clientId}", this.swaggerOptions.ClientId);
        }

        public Stream GetSwaggerJsonDocument(string host, string documentName = "v1")
        {
            var swaggerProvider = this.serviceProvider!.GetRequiredService<ISwaggerProvider>();
            var document = swaggerProvider.GetSwagger(documentName, host, string.Empty);
            return this.SerializeJsonDocument(document);
        }

        public Stream GetSwaggerYamlDocument(string host, string documentName = "v1")
        {
            var swaggerProvider = this.serviceProvider!.GetRequiredService<ISwaggerProvider>();
            var document = swaggerProvider.GetSwagger(documentName, host, string.Empty);
            return this.SerializeYamlDocument(document);
        }

        private static void AddSwaggerDocument(SwaggerGenOptions options, SwaggerDocument document)
        {
            options.SwaggerDoc(document.Name, new OpenApiInfo
            {
                Title = document.Title,
                Version = document.Version,
                Description = document.Description
            });
        }
        private static Assembly GetAssembly()
        {
            var assembly = Assembly.GetAssembly(typeof(SwashBuckleClient));

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return assembly;
        }

        private static string LoadAndUpdateDocument(
            string documentHtml,
            ZipArchive archive,
            string entryName,
            string? replacement = null)
        {
            var entry = archive.GetEntry(entryName);
            using var stream = entry!.Open();
            using var reader = new StreamReader(stream);
            var value = reader.ReadToEnd();

            documentHtml = !string.IsNullOrEmpty(replacement) ?
                documentHtml.Replace(replacement, value) :
                value;

            return documentHtml;
        }

        private MemoryStream SerializeJsonDocument(OpenApiDocument document)
        {
            var memoryStream = new MemoryStream();
            document.SerializeAsJson(
                memoryStream,
                this.swaggerOptions.SpecVersion == OpenApiSpecVersion.OpenApi2_0 ? OpenApiSpecVersion.OpenApi2_0 : OpenApiSpecVersion.OpenApi3_0);

            memoryStream.Position = 0;
            return memoryStream;
        }

        private MemoryStream SerializeYamlDocument(OpenApiDocument document)
        {
            var memoryStream = new MemoryStream();
            document.SerializeAsYaml(
                memoryStream,
                this.swaggerOptions.SpecVersion == OpenApiSpecVersion.OpenApi2_0 ? OpenApiSpecVersion.OpenApi2_0 : OpenApiSpecVersion.OpenApi3_0);

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
