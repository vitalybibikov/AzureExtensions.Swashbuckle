using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.OpenApi;
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

                if (operation.RequestBody is not OpenApiRequestBody requestBodyRef)
                {
                    requestBodyRef = new OpenApiRequestBody();
                    operation.RequestBody = requestBodyRef;
                }

                requestBodyRef.Content ??= new Dictionary<string, OpenApiMediaType>();

                if (!requestBodyRef.Content.ContainsKey(MimeType))
                {
                    requestBodyRef.Content[MimeType] = new OpenApiMediaType();
                }

                requestBodyRef.Content[MimeType].Schema ??= new OpenApiSchema();

                var uploadFileName = string.IsNullOrEmpty(uploadFile.Name)
                    ? "uploadedFile"
                    : uploadFile.Name;

                var uploadFileDescription = string.IsNullOrEmpty(uploadFile.Description)
                    ? "File to upload."
                    : uploadFile.Description;

                var fileSchema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Required = new HashSet<string> { uploadFileName }
                };

                fileSchema.Properties ??= new Dictionary<string, IOpenApiSchema>();
                fileSchema.Properties[uploadFileName] = new OpenApiSchema
                {
                    Description = uploadFileDescription,
                    Type = JsonSchemaType.String,
                    Format = "binary"
                };

                var uploadFileMediaType = new OpenApiMediaType
                {
                    Schema = fileSchema
                };

                var newRequestBody = new OpenApiRequestBody();
                newRequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();
                newRequestBody.Content[MimeType] = uploadFileMediaType;
                operation.RequestBody = newRequestBody;

                if (!string.IsNullOrEmpty(uploadFile.Example) && fileSchema is OpenApiSchema concreteSchema)
                {
                    concreteSchema.Example = JsonValue.Create(uploadFile.Example);
                    concreteSchema.Description = uploadFile.Example;
                }
            }
        }
    }
}
