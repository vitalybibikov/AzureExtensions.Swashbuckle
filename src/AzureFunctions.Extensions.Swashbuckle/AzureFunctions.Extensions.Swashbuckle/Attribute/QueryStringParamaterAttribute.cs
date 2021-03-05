using Microsoft.OpenApi.Any;
using System;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute

{

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class QueryStringParameterAttribute : System.Attribute

    { 
        public QueryStringParameterAttribute(string name, string description)

        {

            Initialise(name, description);
            Example = new OpenApiNull();
        }

        public QueryStringParameterAttribute(string name, string description, string example)
        {

            Initialise(name, description);
            DataType = typeof(string);
            Example = new OpenApiString(example);
        }

        public QueryStringParameterAttribute(string name, string description, int example)
        {
            Initialise(name, description);
            DataType = typeof(int);
            Example = new OpenApiInteger(example);
        }

        public QueryStringParameterAttribute(string name, string description, long example)
        {

            Initialise(name, description);
            DataType = typeof(long);
            Example = new OpenApiLong(example);
        }

        public QueryStringParameterAttribute(string name, string description, double example)
        {
            Initialise(name, description);
            Example = new OpenApiDouble(example);
        }

        public QueryStringParameterAttribute(string name, string description, float example)
        { 
            Initialise(name, description);
            DataType = typeof(float);
            Example = new OpenApiFloat(example);
        }

        public QueryStringParameterAttribute(string name, string description, byte example)
        {
            Initialise(name, description);
            DataType = typeof(byte);
            Example = new OpenApiByte(example);
        }

        public QueryStringParameterAttribute(string name, string description, bool example)
        {
            Initialise(name, description);
            DataType = typeof(bool);
            Example = new OpenApiBoolean(example);
        }

        private void Initialise(string name, string description)
        {
            Name = name;
            Description = description;
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

