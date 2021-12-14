using System;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{

    [AttributeUsage(AttributeTargets.Parameter)]
    public class SwashBuckleClientAttribute : InputBindingAttribute
    {

    }
}