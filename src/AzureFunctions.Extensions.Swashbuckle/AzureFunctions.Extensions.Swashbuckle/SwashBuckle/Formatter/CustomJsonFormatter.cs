using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Formatter
{
    public class CustomJsonFormatter : TextOutputFormatter
    {
        public CustomJsonFormatter(JsonSerializerOptions jsonSerializerOptions)
        {
            SerializerOptions = jsonSerializerOptions;

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add("application/json");
            SupportedMediaTypes.Add("text/json");
            SupportedMediaTypes.Add("application/*+json");
        }

        internal static CustomJsonFormatter CreateFormatter(JsonOptions jsonOptions)
        {
            var jsonSerializerOptions = jsonOptions.JsonSerializerOptions;
            return new CustomJsonFormatter(jsonSerializerOptions);
        }

        public JsonSerializerOptions SerializerOptions { get; }

        public sealed override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (selectedEncoding == null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            var httpContext = context.HttpContext;

            var writeStream = GetWriteStream(httpContext, selectedEncoding);
            try
            {
                var objectType = context.Object?.GetType() ?? context.ObjectType;
                await JsonSerializer.SerializeAsync(writeStream, context.Object, objectType, SerializerOptions);
                await writeStream.FlushAsync();
            }
            finally
            {
                await writeStream.DisposeAsync();
            }
        }

        private Stream GetWriteStream(HttpContext httpContext, Encoding selectedEncoding)
        {
            return httpContext.Response.Body;
        }
    }
}
