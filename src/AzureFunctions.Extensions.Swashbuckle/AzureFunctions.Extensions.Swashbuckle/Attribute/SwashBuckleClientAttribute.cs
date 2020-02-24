using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SwashBuckleClientAttribute : System.Attribute
    {
    }
}