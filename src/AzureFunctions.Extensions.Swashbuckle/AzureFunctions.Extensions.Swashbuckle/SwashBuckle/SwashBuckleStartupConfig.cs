using System.Reflection;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle
{
    internal class SwashBuckleStartupConfig
    {
        public Assembly Assembly { get; set; }

        public bool SerializeAsV2 { get; set; } = false;
    }
}