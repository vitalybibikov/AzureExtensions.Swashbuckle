using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{

    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.DeclaringType != null)
            {
                //if (!(operation?.RequestBody?.Content?.Any(x => x.Key.ToLower() == "multipart/form-data") ?? false)) return;

                var uploadFiles = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                    .Union(context.MethodInfo.GetCustomAttributes(true))
                    .OfType<SwaggerUploadFileAttribute>();

                var swaggerUploadFiles = uploadFiles.ToList();

                if (!swaggerUploadFiles.Any())
                {
                    return;
                }

                var uploadFile = swaggerUploadFiles.First();

                if (operation.RequestBody == null)
                {
                    operation.RequestBody = new OpenApiRequestBody();
                }

                if (!operation.RequestBody.Content.ContainsKey("multipart/form-data"))
                {
                    operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType();
                }

                if (operation.RequestBody.Content["multipart/form-data"].Schema == null)
                {
                    operation.RequestBody.Content["multipart/form-data"].Schema = new OpenApiSchema();
                }

                //operation.RequestBody.Content["multipart/form-data"].Schema.Properties =
                //    new Dictionary<string, OpenApiSchema>
                //    {
                //        {
                //            "upload",
                //            new OpenApiSchema 
                //            {
                //                Type = "object",
                //                Format = "binary",
                //                Description = uploadFile.Description
                //            }
                //        }
                //    };

                var uploadFileMediaType = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema()
                    {
                        Type = "object",
                        Properties =
                        {
                            ["uploadedFile"] = new OpenApiSchema()
                            {
                                Description = "Upload File",
                                Type = "file",
                                Format = "binary"
                            }
                        },
                        Required = new HashSet<string>()
                        {
                            "uploadedFile"
                        }
                    }
                };

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = uploadFileMediaType
                    }
                };


                if (!string.IsNullOrEmpty(uploadFile.Example))
                {
                    operation.RequestBody.Content["multipart/form-data"].Schema.Example =
                        new OpenApiString(uploadFile.Example);
                    operation.RequestBody.Content["multipart/form-data"].Schema.Description = uploadFile.Example;
                }
            }
        }
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SwaggerUploadFileAttribute : System.Attribute
    {
        public SwaggerUploadFileAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }

        public string Parameter { get; set; }

        public string Description { get; set; }

        public string Example { get; set; }
    }
}
