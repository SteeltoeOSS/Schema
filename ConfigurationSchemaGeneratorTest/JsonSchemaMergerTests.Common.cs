// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using ConfigurationSchemaGenerator;
using Newtonsoft.Json.Schema;

namespace ConfigurationSchemaGeneratorTest;

public sealed partial class JsonSchemaMergerTests
{
    [Fact]
    public void Preserves_const_when_same()
    {
        const string source = """
            {
              "const": {
                "a": 1,
                "b": "2"
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
    public void Preserves_const_when_missing()
    {
        const string source1 = """
            {
              "const": {
                "a": 1,
                "b": "2"
              }
            }
            """;

        const string source2 = """
            {
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "const": {
                "a": 1,
                "b": "2"
              }
            }
            """);
    }

    [Fact]
    public void Fails_on_conflicting_const()
    {
        const string source1 = """
            {
              "const": {
                "a": 1,
                "b": "2"
              }
            }
            """;

        const string source2 = """
            {
              "const": {
                "a": 2,
                "b": "1"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);

        Action action = () => schemaMerger.AddSourceText(source2);

        action.Should().ThrowExactly<JSchemaException>().WithMessage("""
            Conflict in schema at (1,1) in element 'const': '{
              "a": 2,
              "b": "1"
            }' vs '{
              "a": 1,
              "b": "2"
            }'.
            """);
    }

    [Fact]
    public void Preserves_enum_when_same()
    {
        const string source = """
            {
              "enum": [
                "a",
                "b",
                1,
                null
              ]
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Fact]
    public void Unifies_different_enum_values()
    {
        const string source1 = """
            {
              "enum": [
                "One",
                1.0,
                false,
              ]
            }
            """;

        const string source2 = """
            {
              "enum": [
                "Two",
                2.0,
                true,
              ]
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "enum": [
                "One",
                1.0,
                false,
                "Two",
                2.0,
                true
              ]
            }
            """);
    }

    [Theory]
    [InlineData("title", "Example title.")]
    [InlineData("description", "Example description.")]
    [InlineData("default", "Example value")]
    [InlineData("schemaVersion", "1.2.3")]
    [InlineData("contentEncoding", "base64")]
    [InlineData("contentMediaType", "application/json")]
    public void Preserves_metadata_keyword_when_same(string keyword, string value)
    {
        string source = $$"""
            {
              "{{keyword}}": "{{value}}"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Theory]
    [InlineData("title", "Example title.")]
    [InlineData("description", "Example description.")]
    [InlineData("default", "Example value")]
    [InlineData("schemaVersion", "1.2.3")]
    [InlineData("contentEncoding", "base64")]
    [InlineData("contentMediaType", "application/json")]
    public void Preserves_metadata_keyword_when_missing(string keyword, string value)
    {
        string source1 = $$"""
            {
              "{{keyword}}": "{{value}}"
            }
            """;

        const string source2 = """
            {
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "{{keyword}}": "{{value}}"
            }
            """);
    }

    [Theory]
    [InlineData("title", "Example title.", "Other title.", true)]
    [InlineData("description", "Example description.", "Other description.", true)]
    [InlineData("default", "true", "false", false)]
    [InlineData("$schema", "http://schema.com/1", "http://schema.com/2", true)]
    [InlineData("contentEncoding", "base64", "base32", true)]
    [InlineData("contentMediaType", "application/json", "text/javascript", true)]
    public void Fails_on_conflicting_metadata_keyword(string keyword, string value1, string value2, bool valueRequiresQuotes)
    {
        string source1 = $$"""
            {
              "type": "boolean",
              "{{keyword}}": {{(valueRequiresQuotes ? $"\"{value1}\"" : value1)}}
            }
            """;

        string source2 = $$"""
            {
            "type": "boolean",
              "{{keyword}}": {{(valueRequiresQuotes ? $"\"{value2}\"" : value2)}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);

        Action action = () => schemaMerger.AddSourceText(source2);

        action.Should().ThrowExactly<JSchemaException>().WithMessage($"Conflict in schema at (1,1) in element '{keyword}': '{value2}' vs '{value1}'.");
    }

    [Theory]
    [InlineData("false", "false")]
    [InlineData("false", "true")]
    [InlineData("true", "false")]
    public void Preserves_read_write_constraints_when_same(string readOnlyValue, string writeOnlyValue)
    {
        string source = $$"""
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string",
                  "writeOnly": {{writeOnlyValue}},
                  "readOnly": {{readOnlyValue}}
                }
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
    public void Removes_read_write_constraints_when_invalid()
    {
        const string source = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string",
                  "writeOnly": true,
                  "readOnly": true
                }
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string"
                }
              }
            }
            """);
    }

    [Fact]
    public void Removes_read_constraint_when_missing()
    {
        const string source1 = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string",
                  "readOnly": true
                }
              }
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string"
                }
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string"
                }
              }
            }
            """);
    }

    [Fact]
    public void Removes_read_constraint_when_different()
    {
        const string source1 = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string",
                  "readOnly": true
                }
              }
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string",
                  "readOnly": false
                }
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string"
                }
              }
            }
            """);
    }

    [Fact]
    public void Removes_write_constraint_when_missing()
    {
        const string source1 = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string",
                  "writeOnly": true
                }
              }
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string"
                }
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string"
                }
              }
            }
            """);
    }

    [Fact]
    public void Removes_write_constraint_when_different()
    {
        const string source1 = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string",
                  "writeOnly": true
                }
              }
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string",
                  "writeOnly": false
                }
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object",
              "properties": {
                "Example": {
                  "type": "string"
                }
              }
            }
            """);
    }

    [Fact]
    public void Merges_definitions()
    {
        const string source1 = """
            {
              "definitions": {
                "logLevel": {
                  "properties": {
                    "One": {
                      "$ref": "#/definitions/logLevelThreshold"
                    }
                  }
                }
              }
            }
            """;

        const string source2 = """
            {
              "definitions": {
                "logLevel": {
                  "properties": {
                    "Two": {
                      "$ref": "#/definitions/logLevelThreshold"
                    }
                  }
                }
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "definitions": {
                "logLevel": {
                  "properties": {
                    "One": {
                      "$ref": "#/definitions/logLevelThreshold"
                    },
                    "Two": {
                      "$ref": "#/definitions/logLevelThreshold"
                    }
                  }
                }
              }
            }
            """);
    }
}
