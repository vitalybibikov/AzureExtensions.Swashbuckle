// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters.Mapper;
using FluentAssertions;
using Microsoft.OpenApi;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class TypeMapperTests
{
    [Fact]
    public void ToOpenApiSpecType_Bool_ReturnsBooleanType()
    {
        var schema = typeof(bool).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Boolean);
        schema.Format.Should().BeNull();
    }

    [Fact]
    public void ToOpenApiSpecType_Byte_ReturnsStringWithByteFormat()
    {
        var schema = typeof(byte).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().Be("byte");
    }

    [Fact]
    public void ToOpenApiSpecType_Int_ReturnsIntegerWithInt32Format()
    {
        var schema = typeof(int).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer);
        schema.Format.Should().Be("int32");
    }

    [Fact]
    public void ToOpenApiSpecType_UInt_ReturnsIntegerWithInt32Format()
    {
        var schema = typeof(uint).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer);
        schema.Format.Should().Be("int32");
    }

    [Fact]
    public void ToOpenApiSpecType_UShort_ReturnsIntegerWithInt32Format()
    {
        var schema = typeof(ushort).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer);
        schema.Format.Should().Be("int32");
    }

    [Fact]
    public void ToOpenApiSpecType_Long_ReturnsIntegerWithInt64Format()
    {
        var schema = typeof(long).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer);
        schema.Format.Should().Be("int64");
    }

    [Fact]
    public void ToOpenApiSpecType_ULong_ReturnsIntegerWithInt64Format()
    {
        var schema = typeof(ulong).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer);
        schema.Format.Should().Be("int64");
    }

    [Fact]
    public void ToOpenApiSpecType_Float_ReturnsNumberWithFloatFormat()
    {
        var schema = typeof(float).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Number);
        schema.Format.Should().Be("float");
    }

    [Fact]
    public void ToOpenApiSpecType_Double_ReturnsNumberWithDoubleFormat()
    {
        var schema = typeof(double).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Number);
        schema.Format.Should().Be("double");
    }

    [Fact]
    public void ToOpenApiSpecType_Decimal_ReturnsNumberWithDoubleFormat()
    {
        var schema = typeof(decimal).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Number);
        schema.Format.Should().Be("double");
    }

    [Fact]
    public void ToOpenApiSpecType_DateTime_ReturnsStringWithDateTimeFormat()
    {
        var schema = typeof(DateTime).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().Be("date-time");
    }

    [Fact]
    public void ToOpenApiSpecType_DateTimeOffset_ReturnsStringWithDateTimeFormat()
    {
        var schema = typeof(DateTimeOffset).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().Be("date-time");
    }

    [Fact]
    public void ToOpenApiSpecType_Guid_ReturnsStringWithUuidFormat()
    {
        var schema = typeof(Guid).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().Be("uuid");
    }

    [Fact]
    public void ToOpenApiSpecType_Char_ReturnsStringType()
    {
        var schema = typeof(char).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().BeNull();
    }

    [Fact]
    public void ToOpenApiSpecType_String_ReturnsStringType()
    {
        var schema = typeof(string).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().BeNull();
    }

    [Fact]
    public void ToOpenApiSpecType_Uri_ReturnsStringType()
    {
        var schema = typeof(Uri).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().BeNull();
    }

    [Fact]
    public void ToOpenApiSpecType_Object_ReturnsObjectType()
    {
        var schema = typeof(object).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Object);
        schema.Format.Should().BeNull();
    }

    // Nullable types

    [Fact]
    public void ToOpenApiSpecType_NullableBool_ReturnsBooleanOrNullType()
    {
        var schema = typeof(bool?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Boolean | JsonSchemaType.Null);
        schema.Format.Should().BeNull();
    }

    [Fact]
    public void ToOpenApiSpecType_NullableByte_ReturnsStringOrNullWithByteFormat()
    {
        var schema = typeof(byte?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String | JsonSchemaType.Null);
        schema.Format.Should().Be("byte");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableInt_ReturnsIntegerOrNullWithInt32Format()
    {
        var schema = typeof(int?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer | JsonSchemaType.Null);
        schema.Format.Should().Be("int32");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableUInt_ReturnsIntegerOrNullWithInt32Format()
    {
        var schema = typeof(uint?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer | JsonSchemaType.Null);
        schema.Format.Should().Be("int32");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableUShort_ReturnsIntegerOrNullWithInt32Format()
    {
        var schema = typeof(ushort?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer | JsonSchemaType.Null);
        schema.Format.Should().Be("int32");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableLong_ReturnsIntegerOrNullWithInt64Format()
    {
        var schema = typeof(long?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer | JsonSchemaType.Null);
        schema.Format.Should().Be("int64");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableULong_ReturnsIntegerOrNullWithInt64Format()
    {
        var schema = typeof(ulong?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Integer | JsonSchemaType.Null);
        schema.Format.Should().Be("int64");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableFloat_ReturnsNumberOrNullWithFloatFormat()
    {
        var schema = typeof(float?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Number | JsonSchemaType.Null);
        schema.Format.Should().Be("float");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableDouble_ReturnsNumberOrNullWithDoubleFormat()
    {
        var schema = typeof(double?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Number | JsonSchemaType.Null);
        schema.Format.Should().Be("double");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableDecimal_ReturnsNumberOrNullWithDoubleFormat()
    {
        var schema = typeof(decimal?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.Number | JsonSchemaType.Null);
        schema.Format.Should().Be("double");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableDateTime_ReturnsStringOrNullWithDateTimeFormat()
    {
        var schema = typeof(DateTime?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String | JsonSchemaType.Null);
        schema.Format.Should().Be("date-time");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableDateTimeOffset_ReturnsStringOrNullWithDateTimeFormat()
    {
        var schema = typeof(DateTimeOffset?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String | JsonSchemaType.Null);
        schema.Format.Should().Be("date-time");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableGuid_ReturnsStringOrNullWithUuidFormat()
    {
        var schema = typeof(Guid?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String | JsonSchemaType.Null);
        schema.Format.Should().Be("uuid");
    }

    [Fact]
    public void ToOpenApiSpecType_NullableChar_ReturnsStringOrNullType()
    {
        var schema = typeof(char?).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String | JsonSchemaType.Null);
        schema.Format.Should().BeNull();
    }

    [Fact]
    public void ToOpenApiSpecType_UnknownType_FallsBackToString()
    {
        var schema = typeof(TypeMapperTests).ToOpenApiSpecType();

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().BeNull();
    }

    [Fact]
    public void ToOpenApiSpecType_NullType_ThrowsArgumentNullException()
    {
        Type nullType = null!;

        var act = () => nullType.ToOpenApiSpecType();

        act.Should().Throw<ArgumentNullException>();
    }
}
