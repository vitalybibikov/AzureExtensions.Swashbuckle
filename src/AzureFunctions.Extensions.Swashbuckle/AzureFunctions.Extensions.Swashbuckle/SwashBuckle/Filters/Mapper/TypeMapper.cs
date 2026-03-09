using System;
using System.Collections.Generic;
using Microsoft.OpenApi;

namespace AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters.Mapper
{
    public static class TypeMapper
    {
        private static readonly Dictionary<Type, Func<OpenApiSchema>> SimpleTypeToOpenApiSchema =
            new Dictionary<Type, Func<OpenApiSchema>>
            {
                [typeof(bool)] = () => new OpenApiSchema { Type = JsonSchemaType.Boolean },
                [typeof(byte)] = () => new OpenApiSchema { Type = JsonSchemaType.String, Format = "byte" },
                [typeof(int)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
                [typeof(uint)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
                [typeof(ushort)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
                [typeof(long)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" },
                [typeof(ulong)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" },
                [typeof(float)] = () => new OpenApiSchema { Type = JsonSchemaType.Number, Format = "float" },
                [typeof(double)] = () => new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" },
                [typeof(decimal)] = () => new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" },
                [typeof(DateTime)] = () => new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" },
                [typeof(DateTimeOffset)] = () => new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" },
                [typeof(Guid)] = () => new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" },
                [typeof(char)] = () => new OpenApiSchema { Type = JsonSchemaType.String },

                [typeof(bool?)] = () => new OpenApiSchema { Type = JsonSchemaType.Boolean | JsonSchemaType.Null },
                [typeof(byte?)] = () => new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "byte" },
                [typeof(int?)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer | JsonSchemaType.Null, Format = "int32" },
                [typeof(uint?)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer | JsonSchemaType.Null, Format = "int32" },
                [typeof(ushort?)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer | JsonSchemaType.Null, Format = "int32" },
                [typeof(long?)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer | JsonSchemaType.Null, Format = "int64" },
                [typeof(ulong?)] = () => new OpenApiSchema { Type = JsonSchemaType.Integer | JsonSchemaType.Null, Format = "int64" },
                [typeof(float?)] = () => new OpenApiSchema { Type = JsonSchemaType.Number | JsonSchemaType.Null, Format = "float" },
                [typeof(double?)] = () => new OpenApiSchema { Type = JsonSchemaType.Number | JsonSchemaType.Null, Format = "double" },
                [typeof(decimal?)] = () => new OpenApiSchema { Type = JsonSchemaType.Number | JsonSchemaType.Null, Format = "double" },
                [typeof(DateTime?)] = () => new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "date-time" },
                [typeof(DateTimeOffset?)] = () =>
                    new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "date-time" },
                [typeof(Guid?)] = () => new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = "uuid" },
                [typeof(char?)] = () => new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null },

                // Uri is treated as simple string.
                [typeof(Uri)] = () => new OpenApiSchema { Type = JsonSchemaType.String },

                [typeof(string)] = () => new OpenApiSchema { Type = JsonSchemaType.String },

                [typeof(object)] = () => new OpenApiSchema { Type = JsonSchemaType.Object }
            };

        public static OpenApiSchema ToOpenApiSpecType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return SimpleTypeToOpenApiSchema.TryGetValue(type, out var result)
                ? result()
                : new OpenApiSchema { Type = JsonSchemaType.String };
        }
    }
}
