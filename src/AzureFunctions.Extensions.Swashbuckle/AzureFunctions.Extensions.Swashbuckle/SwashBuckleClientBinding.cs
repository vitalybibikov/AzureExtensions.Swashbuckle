using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace AzureFunctions.Extensions.Swashbuckle
{
    internal class SwashBuckleClientBinding : IBinding
    {
        private readonly Type _type;
        private readonly SwashbuckleConfig _config;

        public SwashBuckleClientBinding(SwashbuckleConfig config, Type type)
        {
            _config = config;
            _type = type;
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            return Task.FromResult((IValueProvider) new SwashBuckleClientValueProvider(value));
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            var client = new SwashBuckleClient(_config);
            return BindAsync(client, context.ValueContext);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }

        public bool FromAttribute => true;

        private class SwashBuckleClientValueProvider : IValueProvider
        {
            private readonly object value;

            public SwashBuckleClientValueProvider(object value) => this.value = value;

            public Type Type => value.GetType();

            public Task<object> GetValueAsync() => Task.FromResult(value);

            public string ToInvokeString() => value.ToString();
        }
    }

    
}
