using System;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class QueryStringParameterAttribute : System.Attribute
    {
        public QueryStringParameterAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public Type DataType { get; set; }
        public string Description { get; }
        public bool Required { get; set; } = false;
    }
}