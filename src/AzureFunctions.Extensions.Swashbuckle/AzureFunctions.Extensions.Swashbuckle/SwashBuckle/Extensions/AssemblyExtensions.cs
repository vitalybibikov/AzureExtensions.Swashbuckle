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

            var stream = assembly.GetManifestResourceStream($"{typeof(ISwashBuckleClient).Namespace}.{name}");
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            #pragma warning disable CS8603 // Possible null reference return.
            return stream;
            #pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
