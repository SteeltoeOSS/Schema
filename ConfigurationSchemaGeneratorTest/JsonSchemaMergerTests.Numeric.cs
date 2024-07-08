// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using ConfigurationSchemaGenerator;

namespace ConfigurationSchemaGeneratorTest;

public sealed partial class JsonSchemaMergerTests
{
    [Theory]
    [InlineData("multipleOf", "2.0", "integer")]
    [InlineData("multipleOf", "2.0", "number")]
    [InlineData("minimum", "1.0", "integer")]
    [InlineData("minimum", "1.0", "number")]
    [InlineData("maximum", "2.0", "integer")]
    [InlineData("maximum", "2.0", "number")]
    public void Preserves_numeric_constraint_when_same(string keyword, string value, string schemaType)
    {
        string source = $$"""
            {
              "type": "{{schemaType}}",
              "{{keyword}}": {{value}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Theory]
    [InlineData("minimum", "1.0", "exclusiveMaximum", "100.0", "integer")]
    [InlineData("minimum", "1.0", "exclusiveMaximum", "100.0", "number")]
    [InlineData("exclusiveMinimum", "1.0", "maximum", "100.0", "integer")]
    [InlineData("exclusiveMinimum", "1.0", "maximum", "100.0", "number")]
    public void Preserves_numeric_constraint_when_same_range(string minKeyword, string minValue, string maxKeyword, string maxValue, string schemaType)
    {
        string source = $$"""
            {
              "type": "{{schemaType}}",
              "{{minKeyword}}": {{minValue}},
              "{{maxKeyword}}": {{maxValue}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Theory]
    [InlineData("multipleOf", "2.0", "integer")]
    [InlineData("multipleOf", "2.0", "number")]
    [InlineData("minimum", "1.0", "integer")]
    [InlineData("minimum", "1.0", "number")]
    [InlineData("maximum", "2.0", "integer")]
    [InlineData("maximum", "2.0", "number")]
    public void Removes_numeric_constraint_when_missing(string keyword, string value, string schemaType)
    {
        string source1 = $$"""
            {
              "type": "{{schemaType}}",
              "{{keyword}}": {{value}}
            }
            """;

        string source2 = $$"""
            {
              "type": "{{schemaType}}"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "type": "{{schemaType}}"
            }
            """);
    }

    [Theory]
    [InlineData("multipleOf", "2.0", "4.0", "integer")]
    [InlineData("multipleOf", "2.0", "4.0", "number")]
    [InlineData("minimum", "1.0", "2.0", "integer")]
    [InlineData("minimum", "1.0", "2.0", "number")]
    [InlineData("maximum", "2.0", "4.0", "integer")]
    [InlineData("maximum", "2.0", "4.0", "number")]
    public void Removes_numeric_constraint_when_different(string keyword, string value1, string value2, string schemaType)
    {
        string source1 = $$"""
            {
              "type": "{{schemaType}}",
              "{{keyword}}": {{value1}}
            }
            """;

        string source2 = $$"""
            {
              "type": "{{schemaType}}",
              "{{keyword}}": {{value2}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "type": "{{schemaType}}"
            }
            """);
    }

    [Theory]
    [InlineData("minimum", "1.0", "exclusiveMaximum", "100.0", "maximum", true, "integer")]
    [InlineData("minimum", "1.0", "exclusiveMaximum", "100.0", "maximum", true, "number")]
    [InlineData("exclusiveMinimum", "1.0", "maximum", "100.0", "minimum", false, "integer")]
    [InlineData("exclusiveMinimum", "1.0", "maximum", "100.0", "minimum", false, "number")]
    public void Removes_numeric_constraint_when_different_range(string minKeyword, string minValue, string maxKeyword, string maxValue, string alternateKeyword,
        bool alternateKeywordIsMaximum, string schemaType)
    {
        string source1 = $$"""
            {
              "type": "{{schemaType}}",
              "{{minKeyword}}": {{minValue}},
              "{{maxKeyword}}": {{maxValue}}
            }
            """;

        string source2 = $$"""
            {
            "type": "{{schemaType}}",
            "{{(alternateKeywordIsMaximum ? minKeyword : alternateKeyword)}}": {{minValue}},
            "{{(alternateKeywordIsMaximum ? alternateKeyword : maxKeyword)}}": {{maxValue}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "type": "{{schemaType}}",
              "{{(alternateKeywordIsMaximum ? minKeyword : maxKeyword)}}": {{(alternateKeywordIsMaximum ? minValue : maxValue)}}
            }
            """);
    }
}
