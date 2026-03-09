using System;
using System.Text.Json.Nodes;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class QueryStringParameterAttribute : System.Attribute
    {
        public QueryStringParameterAttribute(string name, string description)
        {
            this.Initialize(name, description);
            this.Example = null;
        }

        public QueryStringParameterAttribute(string name, string description, string example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(string);
            this.Example = JsonValue.Create(example);
        }

        public QueryStringParameterAttribute(string name, string description, int example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(int);
            this.Example = JsonValue.Create(example);
        }

        public QueryStringParameterAttribute(string name, string description, long example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(long);
            this.Example = JsonValue.Create(example);
        }

        public QueryStringParameterAttribute(string name, string description, double example)
        {
            this.Initialize(name, description);
            this.Example = JsonValue.Create(example);
        }

        public QueryStringParameterAttribute(string name, string description, float example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(float);
            this.Example = JsonValue.Create(example);
        }

        public QueryStringParameterAttribute(string name, string description, byte example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(byte);
            this.Example = JsonValue.Create(example);
        }

        public QueryStringParameterAttribute(string name, string description, bool example)
        {
            this.Initialize(name, description);
            this.DataType = typeof(bool);
            this.Example = JsonValue.Create(example);
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
        public JsonNode? Example { get; }
    }
}
