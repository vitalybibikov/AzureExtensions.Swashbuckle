using System;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SupportedRequestFormatAttribute : System.Attribute
    {
        public SupportedRequestFormatAttribute(string mediaType)
        {
            MediaType = mediaType;
        }

        public string MediaType { get; }
    }
}