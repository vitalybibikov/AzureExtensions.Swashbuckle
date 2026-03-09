// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using AzureFunctions.Extensions.Swashbuckle.Attribute;
using FluentAssertions;
using Xunit;

namespace AzureFunctions.Extensions.Swashbuckle.Tests;

public class QueryStringParameterAttributeTests
{
    [Fact]
    public void Constructor_WithNameAndDescription_SetsPropertiesCorrectly()
    {
        var attr = new QueryStringParameterAttribute("page", "Page number");

        attr.Name.Should().Be("page");
        attr.Description.Should().Be("Page number");
        attr.Example.Should().BeNull();
        attr.Required.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithStringExample_SetsJsonValueString()
    {
        var attr = new QueryStringParameterAttribute("name", "User name", "John");

        attr.Name.Should().Be("name");
        attr.Description.Should().Be("User name");
        attr.DataType.Should().Be(typeof(string));
        attr.Example.Should().NotBeNull();
        attr.Example!.GetValue<string>().Should().Be("John");
    }

    [Fact]
    public void Constructor_WithIntExample_SetsJsonValueInt()
    {
        var attr = new QueryStringParameterAttribute("page", "Page number", 5);

        attr.Name.Should().Be("page");
        attr.Description.Should().Be("Page number");
        attr.DataType.Should().Be(typeof(int));
        attr.Example.Should().NotBeNull();
        attr.Example!.GetValue<int>().Should().Be(5);
    }

    [Fact]
    public void Constructor_WithBoolExample_SetsJsonValueBool()
    {
        var attr = new QueryStringParameterAttribute("active", "Is active", true);

        attr.Name.Should().Be("active");
        attr.Description.Should().Be("Is active");
        attr.DataType.Should().Be(typeof(bool));
        attr.Example.Should().NotBeNull();
        attr.Example!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithFloatExample_SetsJsonValueFloat()
    {
        var attr = new QueryStringParameterAttribute("ratio", "Ratio value", 1.5f);

        attr.Name.Should().Be("ratio");
        attr.Description.Should().Be("Ratio value");
        attr.DataType.Should().Be(typeof(float));
        attr.Example.Should().NotBeNull();
        attr.Example!.GetValue<float>().Should().Be(1.5f);
    }

    [Fact]
    public void Constructor_WithDoubleExample_SetsJsonValueDouble()
    {
        var attr = new QueryStringParameterAttribute("price", "Price value", 99.99);

        attr.Name.Should().Be("price");
        attr.Description.Should().Be("Price value");
        attr.Example.Should().NotBeNull();
        attr.Example!.GetValue<double>().Should().Be(99.99);
    }

    [Fact]
    public void Constructor_WithLongExample_SetsJsonValueLong()
    {
        var attr = new QueryStringParameterAttribute("id", "Record ID", 9876543210L);

        attr.Name.Should().Be("id");
        attr.Description.Should().Be("Record ID");
        attr.DataType.Should().Be(typeof(long));
        attr.Example.Should().NotBeNull();
        attr.Example!.GetValue<long>().Should().Be(9876543210L);
    }

    [Fact]
    public void Constructor_WithByteExample_SetsJsonValueByte()
    {
        var attr = new QueryStringParameterAttribute("level", "Level value", (byte)128);

        attr.Name.Should().Be("level");
        attr.Description.Should().Be("Level value");
        attr.DataType.Should().Be(typeof(byte));
        attr.Example.Should().NotBeNull();
        attr.Example!.GetValue<byte>().Should().Be(128);
    }

    [Fact]
    public void Required_DefaultsToFalse()
    {
        var attr = new QueryStringParameterAttribute("test", "Test param");

        attr.Required.Should().BeFalse();
    }

    [Fact]
    public void Required_CanBeSetToTrue()
    {
        var attr = new QueryStringParameterAttribute("test", "Test param")
        {
            Required = true
        };

        attr.Required.Should().BeTrue();
    }
}
