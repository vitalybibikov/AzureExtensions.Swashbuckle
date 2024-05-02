using System;
using System.Net.Http;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    /// <summary>
    ///     Explicit body type definition for functions with <see cref="HttpRequestMessage" /> as input parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RequestBodyTypeAttribute : System.Attribute
    {
        /// <summary>
        ///     Explicit body type definition for functions with <see cref="HttpRequestMessage" /> as input parameter
        /// </summary>
        /// <param name="bodyType">Body model type</param>
        /// <param name="description">Model description</param>
        public RequestBodyTypeAttribute(Type bodyType, string description)
        {
            this.Type = bodyType;
            this.Description = description;
        }

        /// <summary>
        ///   Gets  Body model type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///   Gets  Body model description
        /// </summary>
        public string Description { get; }
    }
}
