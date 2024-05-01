using System;

namespace AzureFunctions.Extensions.Swashbuckle.Attribute
{
    //[Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SwashBuckleClientAttribute : System.Attribute
    {
    }
}
