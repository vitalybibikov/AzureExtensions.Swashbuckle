using System;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SupportedRequestFormatAttribute : System.Attribute
    {
        public string MediaType { get; }

        public SupportedRequestFormatAttribute(string mediaType)
        {
            MediaType = mediaType;
        }
    }
}
