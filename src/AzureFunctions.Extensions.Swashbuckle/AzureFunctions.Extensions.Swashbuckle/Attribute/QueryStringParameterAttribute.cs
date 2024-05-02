using Microsoft.OpenApi.Any;
using System;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class QueryStringParameterAttribute : System.Attribute
    {
        public QueryStringParameterAttribute(string name, string description)
        {
            this.Initialize(name, description);
            this.Example = new OpenApiNull();
        }

        public QueryStringParameterAttribute(string name, string description, string example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(string);
            this.Example = new OpenApiString(example);
        }

        public QueryStringParameterAttribute(string name, string description, int example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(int);
            this.Example = new OpenApiInteger(example);
        }

        public QueryStringParameterAttribute(string name, string description, long example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(long);
            this.Example = new OpenApiLong(example);
        }

        public QueryStringParameterAttribute(string name, string description, double example)
        {
            this.Initialize(name, description);
            this.Example = new OpenApiDouble(example);
        }

        public QueryStringParameterAttribute(string name, string description, float example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(float);
            this.Example = new OpenApiFloat(example);
        }

        public QueryStringParameterAttribute(string name, string description, byte example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(byte);
            this.Example = new OpenApiByte(example);
        }

        public QueryStringParameterAttribute(string name, string description, bool example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(bool);
            this.Example = new OpenApiBoolean(example);
        }

        private void Initialize(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public string Name { get; set; }
        public Type DataType { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; } = false;
        public IOpenApiAny Example { get; }
    }
}

