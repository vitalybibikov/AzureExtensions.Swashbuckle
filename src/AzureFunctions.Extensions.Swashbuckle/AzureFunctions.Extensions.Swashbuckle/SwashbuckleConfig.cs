using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
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

namespace AzureFunctions.Extensions.Swashbuckle
{
    [Extension("Swashbuckle", "Swashbuckle")]
    internal class SwashbuckleConfig : IExtensionConfigProvider,
        IAsyncConverter<HttpRequestMessage, HttpResponseMessage>
    {
        private static readonly Lazy<string> IndexHtml = new Lazy<string>(() =>
        {
            var indexHtml = "";
            using (var stream = Assembly.GetAssembly(typeof(SwashbuckleConfig))
                .GetManifestResourceStream($"{typeof(SwashbuckleConfig).Namespace}.EmbededResources.index.html"))
            using (var reader = new StreamReader(stream))
            {
                indexHtml = reader.ReadToEnd();
            }

            using (var stream = Assembly.GetAssembly(typeof(SwashbuckleConfig))
                .GetManifestResourceStream($"{typeof(SwashbuckleConfig).Namespace}.EmbededResources.swagger-ui.css"))
            using (var reader = new StreamReader(stream))
            {
                var style = reader.ReadToEnd();
                indexHtml = indexHtml.Replace("{style}", style);
            }

            using (var stream = Assembly.GetAssembly(typeof(SwashbuckleConfig))
                .GetManifestResourceStream(
                    $"{typeof(SwashbuckleConfig).Namespace}.EmbededResources.swagger-ui-bundle.js"))
            using (var reader = new StreamReader(stream))
            {
                var bundlejs = reader.ReadToEnd();
                indexHtml = indexHtml.Replace("{bundle.js}", bundlejs);
            }

            using (var stream = Assembly.GetAssembly(typeof(SwashbuckleConfig))
                .GetManifestResourceStream(
                    $"{typeof(SwashbuckleConfig).Namespace}.EmbededResources.swagger-ui-standalone-preset.js"))
            using (var reader = new StreamReader(stream))
            {
                var presetjs = reader.ReadToEnd();
                indexHtml = indexHtml.Replace("{standalone-preset.js}", presetjs);
            }

            return indexHtml;
        });

        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionGroupCollectionProvider;
        private readonly HttpOptions _httpOptions;

        private readonly Lazy<string> _indexHtmLazy;
        private readonly Option _option;
        private readonly string _xmlPath;
        private ServiceProvider _serviceProvider;


        public SwashbuckleConfig(
            IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            IOptions<Option> functionsOptions,
            SwashBuckleStartupConfig startupConfig,
            IOptions<HttpOptions> httpOptions)
        {
            _apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
            _option = functionsOptions.Value;
            _httpOptions = httpOptions.Value;
            if (!string.IsNullOrWhiteSpace(_option.XmlPath))
            {
                var binPath = Path.GetDirectoryName(startupConfig.Assembly.Location);
                var binDirectory = Directory.CreateDirectory(binPath);
                var xmlBasePath = binDirectory?.Parent?.FullName;
                var xmlPath = Path.Combine(xmlBasePath, _option.XmlPath);
                if (File.Exists(xmlPath))
                {
                    _xmlPath = xmlPath;
                }
            }

            _indexHtmLazy = new Lazy<string>(() => IndexHtml.Value.Replace("{title}", _option.Title));
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
                if (_option.Documents.Length == 0)
                {
                    var defaultDocument = new OptionDocument();
                    AddSwaggerDocument(options, defaultDocument);
                }
                else
                {
                    foreach (var optionDocument in _option.Documents)
                    {
                        AddSwaggerDocument(options, optionDocument);
                    }
                }

                options.DescribeAllEnumsAsStrings();

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
            var html = _indexHtmLazy.Value;
            return html.Replace("{url}", swaggerUrl);
        }

        public Stream GetSwaggerDocument(string host, string documentName = "v1")
        {
            var requiredService = _serviceProvider.GetRequiredService<ISwaggerProvider>();
            var swaggerDocument = requiredService.GetSwagger(documentName, host, string.Empty);
            var mem = new MemoryStream();
            swaggerDocument.SerializeAsJson(mem, OpenApiSpecVersion.OpenApi3_0);
            mem.Position = 0;
            return mem;
        }

        private static void AddSwaggerDocument(SwaggerGenOptions options, OptionDocument document)
        {
            options.SwaggerDoc(document.Name, new OpenApiInfo
            {
                Title = document.Title,
                Version = document.Version,
                Description = document.Description
            });
        }
    }
}