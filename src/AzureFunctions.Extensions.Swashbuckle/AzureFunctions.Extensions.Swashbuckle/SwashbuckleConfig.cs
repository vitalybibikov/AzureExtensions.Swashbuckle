using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace AzureFunctions.Extensions.Swashbuckle
{
    [Extension("Swashbuckle", "Swashbuckle")]
    internal class SwashbuckleConfig : IExtensionConfigProvider, IAsyncConverter<HttpRequestMessage, HttpResponseMessage>
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionGroupCollectionProvider;
        private readonly Option _option;
        private readonly string _xmlPath;
        private ServiceProvider _serviceProvider;
        private readonly HttpOptions _httpOptions;

        public string RoutePrefix => _httpOptions.RoutePrefix;

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
                .GetManifestResourceStream($"{typeof(SwashbuckleConfig).Namespace}.EmbededResources.swagger-ui-bundle.js"))
            using (var reader = new StreamReader(stream))
            {
                var bundlejs = reader.ReadToEnd();
                indexHtml = indexHtml.Replace("{bundle.js}", bundlejs);
            }

            using (var stream = Assembly.GetAssembly(typeof(SwashbuckleConfig))
                .GetManifestResourceStream($"{typeof(SwashbuckleConfig).Namespace}.EmbededResources.swagger-ui-standalone-preset.js"))
            using (var reader = new StreamReader(stream))
            {
                var presetjs = reader.ReadToEnd();
                indexHtml = indexHtml.Replace("{standalone-preset.js}", presetjs);
            }
            return indexHtml;
        });

        private readonly Lazy<string> _indexHtmLazy;


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

        public string GetSwaggerUIContent(string swaggerUrl)
        {
            var html = _indexHtmLazy.Value;
            return html.Replace("{url}", swaggerUrl);
        }

        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<SwashBuckleClientAttribute>()
                .Bind(new SwashBuckleClientBindingProvider(this));

            var services = new ServiceCollection();

            services.AddSingleton<IApiDescriptionGroupCollectionProvider>(_apiDescriptionGroupCollectionProvider);
            services.AddSwaggerGen(options =>
            {
                foreach (var optionDocument in _option.Documents)
                {
                    options.SwaggerDoc(optionDocument.Name, new Info
                    {
                        Title = optionDocument.Title,
                        Version = optionDocument.Version,
                        Description = optionDocument.Description
                    });
                }
                
                options.DescribeAllEnumsAsStrings();

                if(!string.IsNullOrWhiteSpace(_xmlPath))
                {
                    options.IncludeXmlComments(_xmlPath);
                }
                options.OperationFilter<FunctionsOperationFilter>();
                options.OperationFilter<QueryStringParameterAttributeFilter>();
            });            

            _serviceProvider = services.BuildServiceProvider(true);

        }

        public Stream GetSwaggerDocument(string documentName)
        {
            var requiredService = _serviceProvider.GetRequiredService<ISwaggerProvider>();
            var swaggerDocument = requiredService.GetSwagger(documentName);
            var mem = new MemoryStream();
            var streamWriter = new StreamWriter(mem);
            var mvcOptionsAccessor =
                (IOptions<MvcJsonOptions>)_serviceProvider.GetService(typeof(IOptions<MvcJsonOptions>));
            var serializer = SwaggerSerializerFactory.Create(mvcOptionsAccessor);
            serializer.Serialize(streamWriter, swaggerDocument);
            streamWriter.Flush();
            mem.Position = 0;
            return mem;
        }

        public Task<HttpResponseMessage> ConvertAsync(HttpRequestMessage input, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
