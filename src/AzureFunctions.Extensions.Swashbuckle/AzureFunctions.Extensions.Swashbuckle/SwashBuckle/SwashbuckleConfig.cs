using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureFunctions.Extensions.Swashbuckle.FunctionBinding;
using AzureFunctions.Extensions.Swashbuckle.Settings;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Extensions;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters;
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
    internal class SwashbuckleConfig : IExtensionConfigProvider,
        IAsyncConverter<HttpRequestMessage, HttpResponseMessage>
    {
        private const string IndexHtmlName = "EmbededResources.index.html";
        private const string SwaggerUiName = "EmbededResources.swagger-ui.css";
        private const string SwaggerUiJsName = "EmbededResources.swagger-ui-bundle.js";
        private const string SwaggerUiJsPresetName = "EmbededResources.swagger-ui-standalone-preset.js";

        private static readonly Lazy<string> IndexHtml = new Lazy<string>(() =>
        {
            var indexHtml = String.Empty;
            var assembly = GetAssembly();

            indexHtml = LoadAndUpdateDocument(assembly, indexHtml, IndexHtmlName);
            indexHtml = LoadAndUpdateDocument(assembly, indexHtml, SwaggerUiName, "{style}");
            indexHtml = LoadAndUpdateDocument(assembly, indexHtml, SwaggerUiJsName, "{bundle.js}");
            indexHtml = LoadAndUpdateDocument(assembly, indexHtml, SwaggerUiJsPresetName, "{standalone-preset.js}");

            return indexHtml;
        });

        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionGroupCollectionProvider;
        private readonly HttpOptions _httpOptions;

        private readonly Lazy<string> _indexHtmlLazy;
        private readonly SwaggerDocOptions _swaggerOptions;
        private readonly string _xmlPath;
        private ServiceProvider _serviceProvider;

        public SwashbuckleConfig(
            IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            SwaggerDocOptions swaggerDocOptions,
            SwashBuckleStartupConfig startupConfig,
            IOptions<HttpOptions> httpOptions)
        {
            _apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
            _swaggerOptions = swaggerDocOptions;
            _httpOptions = httpOptions.Value;

            if (!string.IsNullOrWhiteSpace(_swaggerOptions.XmlPath))
            {
                var binPath = Path.GetDirectoryName(startupConfig.Assembly.Location);
                var binDirectory = Directory.CreateDirectory(binPath);
                var xmlBasePath = binDirectory?.Parent?.FullName;
                var xmlPath = Path.Combine(xmlBasePath, _swaggerOptions.XmlPath);

                if (File.Exists(xmlPath))
                {
                    _xmlPath = xmlPath;
                }
            }

            _indexHtmlLazy = new Lazy<string>(
                () => IndexHtml.Value.Replace("{title}", _swaggerOptions.Title));
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
                    options.IncludeXmlComments(_xmlPath);
                }

                options.OperationFilter<FunctionsOperationFilter>();
                options.OperationFilter<QueryStringParameterAttributeFilter>();
                options.OperationFilter<GenerateOperationIdFilter>();
            });

            _serviceProvider = services.BuildServiceProvider(true);
        }

        public string GetSwaggerUIContent(string swaggerUrl)
        {
            var html = _indexHtmlLazy.Value;
            return html.Replace("{url}", swaggerUrl);
        }

        public Stream GetSwaggerDocument(string host, string documentName = "v1")
        {
            var requiredService = _serviceProvider.GetRequiredService<ISwaggerProvider>();
            var swaggerDocument = requiredService.GetSwagger(documentName, host, string.Empty);

            return SerializeDocument(swaggerDocument);
        }

        private MemoryStream SerializeDocument(OpenApiDocument document)
        {
            var memoryStream = new MemoryStream();
            document.SerializeAsJson(memoryStream,
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
            Assembly assembly,
            string documentHtml,
            string resourceName,
            string replacement = null)
        {
            using var stream = assembly.GetResourceByName(resourceName);
            using var reader = new StreamReader(stream);
            var value = reader.ReadToEnd();

            documentHtml = !String.IsNullOrEmpty(replacement) ?
                documentHtml.Replace(replacement, value) :
                value;

            return documentHtml;
        }
    }
}