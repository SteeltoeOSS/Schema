// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using ConfigurationSchemaGenerator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfigurationSchemaGeneratorTest;

public sealed class JsonTokenMergerTests
{
    [Theory]
    [InlineData("null")]
    [InlineData("1")]
    [InlineData("1.1")]
    [InlineData("\"one\"")]
    public void Can_merge_identical_values(string value)
    {
        JToken source1 = JToken.Parse($$"""
            {
              "test": {{value}}
            }
            """);

        JToken source2 = JToken.Parse($$"""
            {
              "test": {{value}}
            }
            """);

        var tokenMerger = new JsonTokenMerger();

        JToken result = tokenMerger.Merge(source1, source2);

        string json = ObjectFormatter.FormatObject(result);

        json.Should().BeJson($$"""
            {
              "test": {{value}}
            }
            """);
    }

    [Fact]
    public void Can_merge_objects()
    {
        JToken source1 = JToken.Parse("""
            {
              "KeyOne": "Value1",
              "KeyShared": "Same"
            }
            """);

        JToken source2 = JToken.Parse("""
            {
              "KeyTwo": 2.2,
              "KeyShared": "Same"
            }
            """);

        var tokenMerger = new JsonTokenMerger();

        JToken result = tokenMerger.Merge(source1, source2);

        string json = ObjectFormatter.FormatObject(result);

        json.Should().BeJson("""
            {
              "KeyTwo": 2.2,
              "KeyShared": "Same",
              "KeyOne": "Value1"
            }
            """);
    }

    [Fact]
    public void Can_merge_arrays()
    {
        JToken source1 = JToken.Parse("""
            [
              "One",
              1.1,
              null
            ]
            """);

        JToken source2 = JToken.Parse("""
            [
              "Two",
              2.2,
              null
            ]
            """);

        var tokenMerger = new JsonTokenMerger();

        JToken result = tokenMerger.Merge(source1, source2);

        string json = ObjectFormatter.FormatObject(result);

        json.Should().BeJson("""
            [
              "Two",
              2.2,
              null,
              "One",
              1.1
            ]
            """);
    }

    [Theory]
    [InlineData("true", "\"one\"", "Boolean", "String")]
    [InlineData("1", "\"one\"", "Integer", "String")]
    [InlineData("1.1", "\"one\"", "Float", "String")]
    [InlineData("\"one\"", "false", "String", "Boolean")]
    public void Cannot_merge_different_types(string value1, string value2, string type1, string type2)
    {
        JToken source1 = JToken.Parse($$"""
            {
              "container": {
                "test": {{value1}}
              }
            }
            """);

        JToken source2 = JToken.Parse($$"""
            {
              "container": {
                "test": {{value2}}
              }
            }
            """);

        var tokenMerger = new JsonTokenMerger();

        Action action = () => tokenMerger.Merge(source1, source2);

        action.Should().ThrowExactly<JsonException>().WithMessage(
            $"Conflict in JSON node type at path 'container.test' within schema extension data: '{type1}' vs '{type2}'.");
    }

    [Theory]
    [InlineData("true", "false")]
    [InlineData("1", "2")]
    [InlineData("1.1", "2.2")]
    [InlineData("\"one\"", "\"two\"")]
    public void Cannot_merge_different_values(string value1, string value2)
    {
        JToken source1 = JToken.Parse($$"""
            {
              "container": {
                "test": {{value1}}
              }
            }
            """);

        JToken source2 = JToken.Parse($$"""
            {
              "container": {
                "test": {{value2}}
              }
            }
            """);

        var tokenMerger = new JsonTokenMerger();

        Action action = () => tokenMerger.Merge(source1, source2);

        string text1 = value1.StartsWith('\"') && value1.EndsWith('\"') ? value1[1..^1] : value1;
        string text2 = value2.StartsWith('\"') && value2.EndsWith('\"') ? value2[1..^1] : value2;

        action.Should().ThrowExactly<JsonException>().WithMessage(
            $"Conflict in JSON node at path 'container.test' within schema extension data: '{text1}' vs '{text2}'.");
    }
}
