using System.Reflection;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace AzureFunctions.Extensions.Swashbuckle.FunctionBinding
{
    internal class SwashBuckleClientBindingProvider : IBindingProvider
    {
        private readonly SwashbuckleConfig _config;

        public SwashBuckleClientBindingProvider(SwashbuckleConfig config)
        {
            _config = config;
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            var parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<SwashBuckleClientAttribute>(false);
            var binding = (IBinding) new SwashBuckleClientBinding(_config, context.Parameter.ParameterType);

            return Task.FromResult(binding);
        }
    }
}