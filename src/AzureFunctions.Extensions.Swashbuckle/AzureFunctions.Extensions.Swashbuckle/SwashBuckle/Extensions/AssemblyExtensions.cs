using System;
using System.IO;
using System.Reflection;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Extensions
{
    internal static class AssemblyExtensions
    {
        public static Stream GetResourceByName(this Assembly assembly, string name)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return assembly.GetManifestResourceStream($"{typeof(ISwashBuckleClient).Namespace}.{name}");
        }
    }
}
