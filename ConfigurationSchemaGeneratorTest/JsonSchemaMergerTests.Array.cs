// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using ConfigurationSchemaGenerator;
using Newtonsoft.Json.Schema;

namespace ConfigurationSchemaGeneratorTest;

public sealed partial class JsonSchemaMergerTests
{
    [Fact]
    public void Preserves_single_item_when_same()
    {
        const string source = """
            {
              "type": "array",
              "items": {
                "type": "number",
                "description": "All items must be a number."
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Fact]
    public void Removes_single_item_when_missing()
    {
        const string source1 = """
            {
              "type": "array",
              "items": {
                "type": "number"
              }
            }
            """;

        const string source2 = """
            {
              "type": "array"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Fact]
    public void Unifies_types_in_single_item()
    {
        const string source1 = """
            {
              "type": "array",
              "items": {
                "type": "string"
              }
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "items": {
                "type": "number"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array",
              "items": {
                "type": [
                  "number",
                  "string"
                ]
              }
            }
            """);
    }

    [Fact]
    public void Fails_on_conflicting_description_in_single_item()
    {
        const string source1 = """
            {
              "type": "array",
              "items": {
                "type": "number",
                "description": "All items must be a number in this array."
              }
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "items": {
                "type": "number",
                "description": "All items in this array must be a number."
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);

        Action action = () => schemaMerger.AddSourceText(source2);

        action.Should().ThrowExactly<JSchemaException>().WithMessage(
            "Conflict in schema at (3,12) in element 'description': 'All items in this array must be a number.' vs 'All items must be a number in this array.'.");
    }

    [Fact]
    public void Unifies_types_in_additional_items_schema()
    {
        const string source1 = """
            {
              "type": "array",
              "additionalItems": {
                "type": "string"
              }
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "additionalItems": {
                "type": "number"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array",
              "additionalItems": {
                "type": [
                  "number",
                  "string"
                ]
              }
            }
            """);
    }

    [Fact]
    public void Fails_on_conflicting_description_in_additional_items_schema()
    {
        const string source1 = """
            {
              "type": "array",
              "additionalItems": {
                "type": "string",
                "description": "One"
              }
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "additionalItems": {
                "type": "number",
                "description": "Two"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);

        Action action = () => schemaMerger.AddSourceText(source2);

        action.Should().ThrowExactly<JSchemaException>().WithMessage("Conflict in schema at (3,22) in element 'description': 'Two' vs 'One'.");
    }

    [Theory]
    [InlineData("false")]
    [InlineData("{ \"type\": \"boolean\" }")]
    public void Preserves_additional_items_constraint_when_same(string value)
    {
        string source = $$"""
            {
              "type": "array",
              "additionalItems": {{value}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Fact]
    public void Removes_additional_items_constraint_when_true()
    {
        const string source = """
            {
              "type": "array",
              "additionalItems": true
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("{ \"type\": \"boolean\" }")]
    public void Removes_additional_items_constraint_when_missing(string value)
    {
        string source1 = $$"""
            {
              "type": "array",
              "additionalItems": {{value}}
            }
            """;

        const string source2 = """
            {
              "type": "array"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Fact]
    public void Prefers_additional_items_schema_over_false()
    {
        const string source1 = """
            {
              "type": "array",
              "additionalItems": false
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "additionalItems": {
                "type": "string"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array",
              "additionalItems": {
                "type": "string"
              }
            }
            """);
    }

    [Fact]
    public void Prefers_additional_items_true_over_schema()
    {
        const string source1 = """
            {
              "type": "array",
              "additionalItems": true
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "additionalItems": {
                "type": "string"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Fact]
    public void Prefers_omitted_additional_items_over_schema()
    {
        const string source1 = """
            {
              "type": "array"
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "additionalItems": {
                "type": "string"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Fact]
    public void Prefers_omitted_additional_items_over_false()
    {
        const string source1 = """
            {
              "type": "array",
              "additionalItems": false
            }
            """;

        const string source2 = """
            {
              "type": "array"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Fact]
    public void Preserves_contains_constraint_when_same()
    {
        const string source = """
            {
              "type": "array",
              "contains": {
                "type": "number",
                "description": "At least one item must be a number."
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Fact]
    public void Removes_contains_constraint_when_missing()
    {
        const string source1 = """
            {
              "type": "array",
              "contains": {
                "type": "number"
              }
            }
            """;

        const string source2 = """
            {
              "type": "array"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Fact]
    public void Unifies_types_in_contains_constraint()
    {
        const string source1 = """
            {
              "type": "array",
              "contains": {
                "type": "string"
              }
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "contains": {
                "type": "number"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array",
              "contains": {
                "type": [
                  "number",
                  "string"
                ]
              }
            }
            """);
    }

    [Fact]
    public void Fails_on_conflicting_description_in_contains_constraint()
    {
        const string source1 = """
            {
              "type": "array",
              "contains": {
                "type": "number",
                "description": "At least one item must be a number in this array."
              }
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "contains": {
                "type": "number",
                "description": "At least one item in this array must be a number."
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);

        Action action = () => schemaMerger.AddSourceText(source2);

        action.Should().ThrowExactly<JSchemaException>().WithMessage(
            "Conflict in schema at (3,15) in element 'description': 'At least one item in this array must be a number.' vs 'At least one item must be a number in this array.'.");
    }

    [Theory]
    [InlineData("minContains")]
    [InlineData("maxContains")]
    public void Preserves_constraint_count_constraint_when_same(string keyword)
    {
        string source = $$"""
            {
              "type": "array",
              "{{keyword}}": 2,
              "contains": {
                "type": "integer"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Theory]
    [InlineData("minContains")]
    [InlineData("maxContains")]
    public void Removes_constraint_count_constraint_when_missing(string keyword)
    {
        string source1 = $$"""
            {
              "type": "array",
              "contains": {
                "type": "integer"
              },
              "{{keyword}}": 2
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "contains": {
                "type": "integer"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array",
              "contains": {
                "type": "integer"
              }
            }
            """);
    }

    [Theory]
    [InlineData("minContains", 2, 4, 2)]
    [InlineData("maxContains", 2, 4, 4)]
    public void Unifies_constraint_count_constraint(string keyword, int value1, int value2, int resultValue)
    {
        string source1 = $$"""
            {
              "type": "array",
              "contains": {
                "type": "integer"
              },
              "{{keyword}}": {{value1}}
            }
            """;

        string source2 = $$"""
            {
              "type": "array",
              "contains": {
                "type": "integer"
              },
              "{{keyword}}": {{value2}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "type": "array",
              "{{keyword}}": {{resultValue}},
              "contains": {
                "type": "integer"
              }
            }
            """);
    }

    [Theory]
    [InlineData("minItems")]
    [InlineData("maxItems")]
    public void Preserves_items_count_constraint_when_same(string keyword)
    {
        string source = $$"""
            {
              "type": "array",
              "{{keyword}}": 2
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Theory]
    [InlineData("minItems")]
    [InlineData("maxItems")]
    public void Removes_items_count_constraint_when_missing(string keyword)
    {
        string source1 = $$"""
            {
              "type": "array",
              "{{keyword}}": 2
            }
            """;

        const string source2 = """
            {
              "type": "array"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Theory]
    [InlineData("minItems", 2, 4, 2)]
    [InlineData("maxItems", 2, 4, 4)]
    public void Unifies_items_count_constraint(string keyword, int value1, int value2, int resultValue)
    {
        string source1 = $$"""
            {
              "type": "array",
              "{{keyword}}": {{value1}}
            }
            """;

        string source2 = $$"""
            {
              "type": "array",
              "{{keyword}}": {{value2}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "type": "array",
              "{{keyword}}": {{resultValue}}
            }
            """);
    }

    [Fact]
    public void Preserves_unique_constraint_when_same()
    {
        const string source = """
            {
              "type": "array",
              "uniqueItems": true
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Fact]
    public void Removes_unique_constraint_when_missing()
    {
        const string source1 = """
            {
              "type": "array",
              "uniqueItems": true
            }
            """;

        const string source2 = """
            {
              "type": "array"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }

    [Fact]
    public void Removes_unique_constraint_when_different()
    {
        const string source1 = """
            {
              "type": "array",
              "uniqueItems": true
            }
            """;

        const string source2 = """
            {
              "type": "array",
              "uniqueItems": false
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "array"
            }
            """);
    }
}
