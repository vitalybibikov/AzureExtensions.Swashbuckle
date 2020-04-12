using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters
{
    internal class GenerateOperationIdFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor
                && !string.IsNullOrEmpty((context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)
                    .ActionName))
            {
                operation.OperationId =
                    (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor).ActionName;
            }
        }
    }
}