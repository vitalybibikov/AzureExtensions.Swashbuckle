using System;
using System.Collections.Generic;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters.Mapper
{
    public static class TypeMapper
    {
        private static readonly Dictionary<Type, string> Map = new Dictionary<Type, string>
        {
            {
                typeof(int), "integer"
            },
            {
                typeof(byte), "integer"
            },
            {
                typeof(long), "integer"
            },
            {
                typeof(double), "number"
            },
            {
                typeof(float), "number"
            },
            {
                typeof(bool), "boolean"
            }
        };

        public static string ToOpenApiSpecType(this Type type)
        {
            return type != null ? Map.ContainsKey(type) ? Map[type] : type.ToString() : String.Empty;
        }
    }
}
