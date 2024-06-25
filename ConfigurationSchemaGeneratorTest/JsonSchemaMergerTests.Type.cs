// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using ConfigurationSchemaGenerator;

namespace ConfigurationSchemaGeneratorTest;

public sealed partial class JsonSchemaMergerTests
{
    [Fact]
    public void Skips_over_missing_type_constraint()
    {
        const string source1 = """
            {
              "type": "integer",
              "summary": "Summary for one."
            }
            """;

        const string source2 = """
            {
              "description": "Description for two."
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "integer",
              "summary": "Summary for one.",
              "description": "Description for two."
            }
            """);
    }

    [Fact]
    public void Unions_different_type_constraints()
    {
        const string source1 = """
            {
              "type": "boolean"
            }
            """;

        const string source2 = """
            {
              "type": "string"
            }
            """;

        const string source3 = """
            {
              "type": "number"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);
        schemaMerger.AddSourceText(source3);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": [
                "boolean",
                "number",
                "string"
              ]
            }
            """);
    }
}
