using System;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SwaggerUploadFileAttribute : System.Attribute
    {
        public SwaggerUploadFileAttribute(string name, string description, string example = "")
        {
            Name = name;
            Description = description;
            Example = example;
        }

        public string Name { get; }

        public string Description { get; set; }

        public string Example { get; set; }
    }
}
