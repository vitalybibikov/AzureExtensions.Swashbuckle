// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using AzureFunctions.Extensions.Swashbuckle.SwashBuckle.Filters.Mapper;
using FluentAssertions;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

/// <summary>
/// Tests for <see cref="JsonMapper.CreateFromJson"/>.
/// The method serializes the input string via JsonSerializer.Serialize (wrapping it in quotes),
/// then deserializes to JsonElement. Because the input is always a C# string, the deserialized
/// JsonElement is always ValueKind.String — so boolean/number branches are only reachable
/// through the recursive array element path where raw JSON tokens are passed back in.
/// </summary>
public class JsonMapperTests
{
    [Fact]
    public void CreateFromJson_BooleanLiteral_ReturnedAsString()
    {
        // "true" is a C# string → Serialize wraps it → deserialized as String kind
        var result = JsonMapper.CreateFromJson("true");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("true");
    }

    [Fact]
    public void CreateFromJson_IntegerLiteral_ReturnedAsString()
    {
        // "42" is a C# string → Serialize wraps it → deserialized as String kind
        var result = JsonMapper.CreateFromJson("42");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("42");
    }

    [Fact]
    public void CreateFromJson_FloatLiteral_ReturnedAsString()
    {
        var result = JsonMapper.CreateFromJson("3.14");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("3.14");
    }

    [Fact]
    public void CreateFromJson_PlainString_ReturnedAsString()
    {
        var result = JsonMapper.CreateFromJson("hello world");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("hello world");
    }

    [Fact]
    public void CreateFromJson_NullLiteral_ReturnedAsString()
    {
        // "null" is a C# string → Serialize wraps → deserialized as String "null"
        var result = JsonMapper.CreateFromJson("null");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("null");
    }

    [Fact]
    public void CreateFromJson_EmptyString_ReturnedAsEmptyString()
    {
        var result = JsonMapper.CreateFromJson("");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().BeEmpty();
    }

    [Fact]
    public void CreateFromJson_NegativeNumber_ReturnedAsString()
    {
        var result = JsonMapper.CreateFromJson("-10");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("-10");
    }

    [Fact]
    public void CreateFromJson_StringWithSpecialChars_ReturnedCorrectly()
    {
        var result = JsonMapper.CreateFromJson("hello \"world\"");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("hello \"world\"");
    }

    [Fact]
    public void CreateFromJson_ArrayLiteral_ReturnedAsString()
    {
        // "[1, 2, 3]" is a C# string → Serialize wraps it → deserialized as String kind
        var result = JsonMapper.CreateFromJson("[1, 2, 3]");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("[1, 2, 3]");
    }

    [Fact]
    public void CreateFromJson_LongNumberLiteral_ReturnedAsString()
    {
        var longValue = ((long)int.MaxValue + 1).ToString();
        var result = JsonMapper.CreateFromJson(longValue);

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be(longValue);
    }

    [Fact]
    public void CreateFromJson_UnicodeString_ReturnedCorrectly()
    {
        var result = JsonMapper.CreateFromJson("\u00e9\u00e0\u00fc");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("\u00e9\u00e0\u00fc");
    }

    [Fact]
    public void CreateFromJson_WhitespaceOnly_ReturnedAsString()
    {
        var result = JsonMapper.CreateFromJson("   ");

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be("   ");
    }

    [Fact]
    public void CreateFromJson_VeryLongString_ReturnedCorrectly()
    {
        var longString = new string('x', 10000);
        var result = JsonMapper.CreateFromJson(longString);

        result.Should().NotBeNull();
        result!.GetValue<string>().Should().Be(longString);
    }
}
