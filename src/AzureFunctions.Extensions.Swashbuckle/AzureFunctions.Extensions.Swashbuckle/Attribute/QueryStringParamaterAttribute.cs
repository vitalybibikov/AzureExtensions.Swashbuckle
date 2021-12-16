using System;
using Microsoft.OpenApi.Any;

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
            Initialize(name, description);
            DataType = typeof(int);
            Example = new OpenApiInteger(example);
        }

        public QueryStringParameterAttribute(string name, string description, long example)
        {

            Initialize(name, description);
            DataType = typeof(long);
            Example = new OpenApiLong(example);
        }

        public QueryStringParameterAttribute(string name, string description, double example)
        {
            Initialize(name, description);
            Example = new OpenApiDouble(example);
        }

        public QueryStringParameterAttribute(string name, string description, float example)
        { 
            Initialize(name, description);
            DataType = typeof(float);
            Example = new OpenApiFloat(example);
        }

        public QueryStringParameterAttribute(string name, string description, byte example)
        {
            Initialize(name, description);
            DataType = typeof(byte);
            Example = new OpenApiByte(example);
        }

        public QueryStringParameterAttribute(string name, string description, bool example)
        {
            Initialize(name, description);
            DataType = typeof(bool);
            Example = new OpenApiBoolean(example);
        }

        private void Initialize(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        #region public properties

        public string Name { get; set; }
        public Type DataType { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; } = false;
        public IOpenApiAny Example { get; }

        #endregion
    }

}

