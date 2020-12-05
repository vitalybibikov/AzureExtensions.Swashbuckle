using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Formatters
{
    public class SystemTextJsonOutputFormatter2 : TextOutputFormatter
    {
        public SystemTextJsonOutputFormatter2(JsonSerializerOptions jsonSerializerOptions)
        {
            SerializerOptions = jsonSerializerOptions;

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add("application/json");
            SupportedMediaTypes.Add("text/json");
            SupportedMediaTypes.Add("application/*+json");
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

                if (writeStream is TranscodingWriteStream transcodingStream)
                {
                    await transcodingStream.FinalWriteAsync(CancellationToken.None);
                }
                await writeStream.FlushAsync();
            }
            finally
            {
                if (writeStream is TranscodingWriteStream transcodingStream)
                {
                    await transcodingStream.DisposeAsync();
                }
            }
        }

        private Stream GetWriteStream(HttpContext httpContext, Encoding selectedEncoding)
        {
            if (selectedEncoding.CodePage == Encoding.UTF8.CodePage)
            {
                return httpContext.Response.Body;
            }

            return new TranscodingWriteStream(httpContext.Response.Body, selectedEncoding);
        }
    }
}
