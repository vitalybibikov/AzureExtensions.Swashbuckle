using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class QueryStringParamaterAttribute : System.Attribute
    {
        public QueryStringParamaterAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }
        public Type DataType { get; set; }
        public string Description { get; private set; }
        public bool Required { get; set; } = false;
    }
}
