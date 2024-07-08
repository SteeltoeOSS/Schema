// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using ConfigurationSchemaGenerator;

namespace ConfigurationSchemaGeneratorTest;

public sealed partial class JsonSchemaMergerTests
{
    [Theory]
    [InlineData("pattern", "\"^[A-Z]+$\"")]
    [InlineData("format", "\"uuid\"")]
    [InlineData("minLength", "4")]
    [InlineData("maxLength", "8")]
    public void Preserves_string_constraint_when_same(string keyword, string value)
    {
        string source = $$"""
            {
              "type": "string",
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
    [InlineData("pattern", "\"^[A-Z]+$\"")]
    [InlineData("format", "\"uuid\"")]
    [InlineData("minLength", "4")]
    [InlineData("maxLength", "8")]
    public void Removes_string_constraint_when_missing(string keyword, string value)
    {
        string source1 = $$"""
            {
              "type": "string",
              "{{keyword}}": {{value}}
            }
            """;

        const string source2 = """
            {
              "type": "string"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "string"
            }
            """);
    }

    [Theory]
    [InlineData("pattern", "\"^[A-Z]+$\"", "\"^[0-9]+$\"")]
    [InlineData("format", "\"uuid\"", "\"time\"")]
    [InlineData("minLength", "4", "2")]
    [InlineData("maxLength", "8", "4")]
    public void Removes_string_constraint_when_different(string keyword, string value1, string value2)
    {
        string source1 = $$"""
            {
              "type": "string",
              "{{keyword}}": {{value1}}
            }
            """;

        string source2 = $$"""
            {
              "type": "string",
              "{{keyword}}": {{value2}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "string"
            }
            """);
    }
}
