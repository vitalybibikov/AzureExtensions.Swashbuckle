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

                var uploadFileName = string.IsNullOrEmpty(uploadFile.Name)
                    ? "uploadedFile"
                    : uploadFile.Name;

                var uploadFileDescription = string.IsNullOrEmpty(uploadFile.Name)
                    ? "File to upload."
                    : uploadFile.Description;

                var uploadFileMediaType = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            [uploadFileName] = new OpenApiSchema
                            {
                                Description = uploadFileDescription,
                                Type = "file",
                                Format = "binary"
                            }
                        },
                        Required = new HashSet<string>
                        {
                            uploadFileName
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
