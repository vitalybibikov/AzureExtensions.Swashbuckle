using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters.Mapper
{
    public static class TypeMapper
    {
        private static readonly Dictionary<Type, Func<OpenApiSchema>> SimpleTypeToOpenApiSchema =
            new Dictionary<Type, Func<OpenApiSchema>>
            {
                [typeof(bool)] = () => new OpenApiSchema {Type = "boolean"},
                [typeof(byte)] = () => new OpenApiSchema {Type = "string", Format = "byte"},
                [typeof(int)] = () => new OpenApiSchema {Type = "integer", Format = "int32"},
                [typeof(uint)] = () => new OpenApiSchema {Type = "integer", Format = "int32"},
                [typeof(ushort)] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
                [typeof(long)] = () => new OpenApiSchema {Type = "integer", Format = "int64"},
                [typeof(ulong)] = () => new OpenApiSchema {Type = "integer", Format = "int64"},
                [typeof(float)] = () => new OpenApiSchema {Type = "number", Format = "float"},
                [typeof(double)] = () => new OpenApiSchema {Type = "number", Format = "double"},
                [typeof(decimal)] = () => new OpenApiSchema {Type = "number", Format = "double"},
                [typeof(DateTime)] = () => new OpenApiSchema {Type = "string", Format = "date-time"},
                [typeof(DateTimeOffset)] = () => new OpenApiSchema {Type = "string", Format = "date-time"},
                [typeof(Guid)] = () => new OpenApiSchema {Type = "string", Format = "uuid"},
                [typeof(char)] = () => new OpenApiSchema {Type = "string"},

                [typeof(bool?)] = () => new OpenApiSchema {Type = "boolean", Nullable = true},
                [typeof(byte?)] = () => new OpenApiSchema {Type = "string", Format = "byte", Nullable = true},
                [typeof(int?)] = () => new OpenApiSchema {Type = "integer", Format = "int32", Nullable = true},
                [typeof(uint?)] = () => new OpenApiSchema {Type = "integer", Format = "int32", Nullable = true},
                [typeof(ushort?)] = () => new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true },
                [typeof(long?)] = () => new OpenApiSchema {Type = "integer", Format = "int64", Nullable = true},
                [typeof(ulong?)] = () => new OpenApiSchema {Type = "integer", Format = "int64", Nullable = true},
                [typeof(float?)] = () => new OpenApiSchema {Type = "number", Format = "float", Nullable = true},
                [typeof(double?)] = () => new OpenApiSchema {Type = "number", Format = "double", Nullable = true},
                [typeof(decimal?)] = () => new OpenApiSchema {Type = "number", Format = "double", Nullable = true},
                [typeof(DateTime?)] = () => new OpenApiSchema {Type = "string", Format = "date-time", Nullable = true},
                [typeof(DateTimeOffset?)] = () =>
                    new OpenApiSchema {Type = "string", Format = "date-time", Nullable = true},
                [typeof(Guid?)] = () => new OpenApiSchema {Type = "string", Format = "uuid", Nullable = true},
                [typeof(char?)] = () => new OpenApiSchema {Type = "string", Nullable = true},

                // Uri is treated as simple string.
                [typeof(Uri)] = () => new OpenApiSchema {Type = "string"},

                [typeof(string)] = () => new OpenApiSchema {Type = "string"},

                [typeof(object)] = () => new OpenApiSchema {Type = "object"}
            };

        public static OpenApiSchema ToOpenApiSpecType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return SimpleTypeToOpenApiSchema.TryGetValue(type, out var result)
                ? result()
                : new OpenApiSchema {Type = "string"};
        }
    }
}
