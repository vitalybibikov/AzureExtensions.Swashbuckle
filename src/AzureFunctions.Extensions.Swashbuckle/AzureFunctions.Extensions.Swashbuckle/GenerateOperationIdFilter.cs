using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureFunctions.Extensions.Swashbuckle
{
    internal class GenerateOperationIdFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor 
                && !string.IsNullOrEmpty((context.ApiDescription.ActionDescriptor as ControllerActionDescriptor).ActionName))
            {
                operation.OperationId = (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor).ActionName;
            }
        }
    }
}
