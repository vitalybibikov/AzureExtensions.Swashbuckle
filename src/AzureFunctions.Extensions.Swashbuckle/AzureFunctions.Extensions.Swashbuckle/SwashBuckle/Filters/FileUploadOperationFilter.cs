using System;
using System.Collections.Generic;
using System.Linq;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        private const string MimeType = "multipart/form-data";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.DeclaringType != null)
            {
                var uploadFiles = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                    .Union(context.MethodInfo.GetCustomAttributes(true))
                    .OfType<SwaggerUploadFileAttribute>();

                var swaggerUploadFiles = uploadFiles.ToList();

                if (!swaggerUploadFiles.Any())
                {
                    return;
                }

                var uploadFile = swaggerUploadFiles.First();
                operation.RequestBody ??= new OpenApiRequestBody();

                if (!operation.RequestBody.Content.ContainsKey(MimeType))
                {
                    operation.RequestBody.Content[MimeType] = new OpenApiMediaType();
                }

                operation.RequestBody.Content[MimeType].Schema ??= new OpenApiSchema();

                var uploadFileMediaType = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["uploadedFile"] = new OpenApiSchema
                            {
                                Description = "File to upload.",
                                Type = "file",
                                Format = "binary"
                            }
                        },
                        Required = new HashSet<string>
                        {
                            "uploadedFile"
                        }
                    }
                };

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        [MimeType] = uploadFileMediaType
                    }
                };

                if (!string.IsNullOrEmpty(uploadFile.Example))
                {
                    operation.RequestBody.Content[MimeType].Schema.Example =
                        new OpenApiString(uploadFile.Example);
                    operation.RequestBody.Content[MimeType].Schema.Description = uploadFile.Example;
                }
            }
        }
    }
}
