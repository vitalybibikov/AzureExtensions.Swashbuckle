using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureFunctions.Extensions.Swashbuckle.FunctionBinding;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Extensions;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle
{
    [Extension("Swashbuckle", "Swashbuckle")]
    public sealed class SwashbuckleConfig : IExtensionConfigProvider,
        IAsyncConverter<HttpRequestMessage, HttpResponseMessage>
    {
        private const string ZippedResources = "EmbededResources.resources.zip";
        private const string IndexHtmlName = "index.html";
        private const string SwaggerUiName = "swagger-ui.css";
        private const string SwaggerUiJsName = "swagger-ui-bundle.js";
        private const string SwaggerUiJsPresetName = "swagger-ui-standalone-preset.js";
        private const string SwaggerOAuth2RedirectName = "oauth2-redirect.html";

        private static readonly Lazy<string> IndexHtml = new Lazy<string>(() =>
        {
            var indexHtml = String.Empty;
            var assembly = GetAssembly();

            using var stream = assembly.GetResourceByName(ZippedResources);
            using var archive = new ZipArchive(stream);

            indexHtml = LoadAndUpdateDocument(indexHtml, archive, IndexHtmlName);
            indexHtml = LoadAndUpdateDocument(indexHtml, archive, SwaggerUiName, "{style}");
            indexHtml = LoadAndUpdateDocument(indexHtml, archive, SwaggerUiJsName, "{bundle.js}");
            indexHtml = LoadAndUpdateDocument(indexHtml, archive, SwaggerUiJsPresetName, "{standalone-preset.js}");

            return indexHtml;
        });

        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionGroupCollectionProvider;
        private readonly HttpOptions _httpOptions;

        private readonly Lazy<string> _indexHtmlLazy;
        private readonly Lazy<string> _oauth2RedirectLazy;
        private readonly SwaggerDocOptions _swaggerOptions;
        private readonly string _xmlPath;
        private ServiceProvider _serviceProvider;

        public SwashbuckleConfig(
            IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            IOptions<SwaggerDocOptions> swaggerDocOptions,
            SwashBuckleStartupConfig startupConfig,
            IOptions<HttpOptions> httpOptions)
        {
            _apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
            _swaggerOptions = swaggerDocOptions.Value;
            _httpOptions = httpOptions.Value;

            if (!string.IsNullOrWhiteSpace(_swaggerOptions.XmlPath))
            {
                var binPath = Path.GetDirectoryName(startupConfig.Assembly.Location);
                var binDirectory = Directory.CreateDirectory(binPath);
                var xmlBasePath = binDirectory?.Parent?.FullName;
                if (xmlBasePath != null)
                {
                    var xmlPath = Path.Combine(xmlBasePath, _swaggerOptions.XmlPath);

                    if (File.Exists(xmlPath))
                    {
                        _xmlPath = xmlPath;
                    }
                }
            }

            _indexHtmlLazy = new Lazy<string>(
                () => IndexHtml.Value.Replace("{title}", _swaggerOptions.Title));

            _oauth2RedirectLazy = new Lazy<string>(() => {
                var assembly = GetAssembly();
                using var stream = assembly.GetResourceByName(ZippedResources);
                using var archive = new ZipArchive(stream);
                var entry = archive.GetEntry(SwaggerOAuth2RedirectName);

                using var entryStream = entry.Open();
                using var reader = new StreamReader(entryStream);
                return reader.ReadToEnd();
            });
        }

        public string RoutePrefix => _httpOptions.RoutePrefix;

        public Task<HttpResponseMessage> ConvertAsync(HttpRequestMessage input, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<SwashBuckleClientAttribute>()
                .Bind(new SwashBuckleClientBindingProvider(this));

            var services = new ServiceCollection();

            services.AddSingleton(_apiDescriptionGroupCollectionProvider);

            services.AddSingleton<IWebHostEnvironment>(new FunctionHostingEnvironment());

            services.AddSwaggerGen(options =>
            {
                if (!_swaggerOptions.Documents.Any())
                {
                    var defaultDocument = new SwaggerDocument();
                    AddSwaggerDocument(options, defaultDocument);
                }
                else
                {
                    foreach (var optionDocument in _swaggerOptions.Documents)
                    {
                        AddSwaggerDocument(options, optionDocument);
                    }
                }

                if (!string.IsNullOrWhiteSpace(_xmlPath))
                {
                    var xmlDoc = new XPathDocument(_xmlPath);
                    options.IncludeXmlComments(_xmlPath);
                    options.OperationFilter<XmlCommentsOperationFilterWithParams>(xmlDoc);
                    options.ParameterFilter<XmlCommentsParameterFilterWithExamples>(xmlDoc);
                    options.SchemaFilter<XmlCommentsSchemaFilterChanged>(xmlDoc);
                }

                options.OperationFilter<FunctionsOperationFilter>();
                options.OperationFilter<QueryStringParameterAttributeFilter>();
                options.OperationFilter<GenerateOperationIdFilter>();
                options.OperationFilter<FileUploadOperationFilter>();

                _swaggerOptions.ConfigureSwaggerGen?.Invoke(options);
            });

            _serviceProvider = services.BuildServiceProvider(true);
        }



        public string GetSwaggerOAuth2RedirectContent()
        {
            return _oauth2RedirectLazy.Value;
        }

        public string GetSwaggerUIContent(string swaggerUrl)
        {
            if (_swaggerOptions.OverridenPathToSwaggerJson != null)
            {
                swaggerUrl = _swaggerOptions.OverridenPathToSwaggerJson.ToString();
            }

            var html = _indexHtmlLazy.Value;
            return html
                .Replace("{url}", swaggerUrl)
                .Replace("{oauth2RedirectUrl}", _swaggerOptions.OAuth2RedirectPath)
                .Replace("{clientId}", _swaggerOptions.ClientId);
        }

        public Stream GetSwaggerJsonDocument(string host, string documentName = "v1")
        {
            var swaggerProvider = _serviceProvider.GetRequiredService<ISwaggerProvider>();
            var document = swaggerProvider.GetSwagger(documentName, host, string.Empty);
            return SerializeJsonDocument(document);
        }

        public Stream GetSwaggerYamlDocument(string host, string documentName = "v1")
        {
            var swaggerProvider = _serviceProvider.GetRequiredService<ISwaggerProvider>();
            var document = swaggerProvider.GetSwagger(documentName, host, string.Empty);
            return SerializeYamlDocument(document);
        }

        private MemoryStream SerializeJsonDocument(OpenApiDocument document)
        {
            var memoryStream = new MemoryStream();
            document.SerializeAsJson(memoryStream,
                _swaggerOptions.SpecVersion == OpenApiSpecVersion.OpenApi2_0 ?
                    OpenApiSpecVersion.OpenApi2_0 :
                    OpenApiSpecVersion.OpenApi3_0);

            memoryStream.Position = 0;
            return memoryStream;
        }

        private MemoryStream SerializeYamlDocument(OpenApiDocument document)
        {
            var memoryStream = new MemoryStream();
            document.SerializeAsYaml(memoryStream,
                _swaggerOptions.SpecVersion == OpenApiSpecVersion.OpenApi2_0 ?
                    OpenApiSpecVersion.OpenApi2_0 :
                    OpenApiSpecVersion.OpenApi3_0);

            memoryStream.Position = 0;
            return memoryStream;
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
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            var value = reader.ReadToEnd();

            documentHtml = !String.IsNullOrEmpty(replacement) ?
                documentHtml.Replace(replacement, value) :
                value;

            return documentHtml;
        }
    }
}