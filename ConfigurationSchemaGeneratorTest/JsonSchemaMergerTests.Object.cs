// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using ConfigurationSchemaGenerator;
using Newtonsoft.Json.Schema;

namespace ConfigurationSchemaGeneratorTest;

public sealed partial class JsonSchemaMergerTests
{
    [Theory]
    [InlineData("properties")]
    [InlineData("patternProperties")]
    public void Preserves_properties_when_same(string keyword)
    {
        string source = $$"""
            {
              "type": "object",
              "{{keyword}}": {
                "A": {
                  "type": "number",
                  "summary": "Summary for A."
                },
                "B": {
                  "type": "boolean",
                  "description": "Description for B."
                }
              },
              "description": "Outer description."
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Theory]
    [InlineData("properties")]
    [InlineData("patternProperties")]
    public void Unifies_different_properties(string keyword)
    {
        string source1 = $$"""
            {
              "type": "object",
              "{{keyword}}": {
                "A": {
                  "type": "number",
                  "summary": "Summary for A."
                }
              },
              "description": "Outer description."
            }
            """;

        string source2 = $$"""
            {
              "type": "object",
              "{{keyword}}": {
                "B": {
                  "type": "boolean",
                  "description": "Description for B."
                }
              },
              "description": "Outer description."
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "type": "object",
              "{{keyword}}": {
                "A": {
                  "type": "number",
                  "summary": "Summary for A."
                },
                "B": {
                  "type": "boolean",
                  "description": "Description for B."
                }
              },
              "description": "Outer description."
            }
            """);
    }

    [Fact]
    public void Merges_properties_in_subtree()
    {
        const string source1 = """
            {
              "type": "object",
              "properties": {
                "Company": {
                  "type": "object",
                  "properties": {
                    "Product": {
                      "type": "object",
                      "properties": {
                        "One": {
                          "type": "string",
                          "description": "Description for one."
                        }
                      }
                    }
                  }
                }
              }
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "properties": {
                "Company": {
                  "type": "object",
                  "properties": {
                    "Product": {
                      "type": "object",
                      "properties": {
                        "Two": {
                          "type": "string",
                          "description": "Description for two."
                        }
                      }
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
              "type": "object",
              "properties": {
                "Company": {
                  "type": "object",
                  "properties": {
                    "Product": {
                      "type": "object",
                      "properties": {
                        "One": {
                          "type": "string",
                          "description": "Description for one."
                        },
                        "Two": {
                          "type": "string",
                          "description": "Description for two."
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
    }

    [Fact]
    public void Merges_subtree_of_properties()
    {
        const string source1 = """
            {
              "type": "object",
              "properties": {
                "Company": {
                  "type": "object",
                  "properties": {
                    "ProductA": {
                      "type": "object",
                      "properties": {
                        "One": {
                          "type": "string",
                          "description": "Description for one."
                        }
                      }
                    }
                  }
                }
              }
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "properties": {
                "Company": {
                  "type": "object",
                  "properties": {
                    "ProductB": {
                      "type": "object",
                      "properties": {
                        "Two": {
                          "description": "Description for two.",
                          "type": "string"
                        }
                      }
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
              "type": "object",
              "properties": {
                "Company": {
                  "type": "object",
                  "properties": {
                    "ProductA": {
                      "type": "object",
                      "properties": {
                        "One": {
                          "type": "string",
                          "description": "Description for one."
                        }
                      }
                    },
                    "ProductB": {
                      "type": "object",
                      "properties": {
                        "Two": {
                          "type": "string",
                          "description": "Description for two."
                        }
                      }
                    }
                  }
                }
              }
            }
            """);
    }

    [Theory]
    [InlineData("properties")]
    [InlineData("patternProperties")]
    public void Unifies_keywords_in_same_property(string keyword)
    {
        string source1 = $$"""
            {
              "type": "object",
              "{{keyword}}": {
                "Test": {
                  "type": "number",
                  "summary": "Summary for Test."
                }
              },
              "description": "Outer description."
            }
            """;

        string source2 = $$"""
            {
              "type": "object",
              "{{keyword}}": {
                "Test": {
                  "type": "number",
                  "description": "Description for Test."
                }
              },
              "description": "Outer description."
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "type": "object",
              "{{keyword}}": {
                "Test": {
                  "type": "number",
                  "summary": "Summary for Test.",
                  "description": "Description for Test."
                }
              },
              "description": "Outer description."
            }
            """);
    }

    [Theory]
    [InlineData("properties")]
    [InlineData("patternProperties")]
    public void Fails_on_conflicting_description_in_property(string keyword)
    {
        string source1 = $$"""
            {
              "type": "object",
              "{{keyword}}": {
                "Test": {
                  "type": "number",
                  "description": "Description for Test."
                }
              }
            }
            """;

        string source2 = $$"""
            {
              "type": "object",
              "{{keyword}}": {
                "Test": {
                  "type": "number",
                  "description": "Alternative description for Test."
                }
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);

        Action action = () => schemaMerger.AddSourceText(source2);

        action.Should().ThrowExactly<JSchemaException>().WithMessage(
            "Conflict in schema at (4,13) in element 'description': 'Alternative description for Test.' vs 'Description for Test.'.");
    }

    [Fact]
    public void Preserves_required_when_same()
    {
        const string source = """
            {
              "type": "object",
              "properties": {
                "A": {
                  "type": "string"
                },
                "B": {
                  "type": "string"
                }
              },
              "required": [
                "A",
                "B"
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
    public void Intersects_required()
    {
        const string source1 = """
            {
              "type": "object",
              "properties": {
                "A": {
                  "type": "string"
                },
                "B": {
                  "type": "string"
                }
              },
              "required": [
                "A",
                "B"
              ]
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "properties": {
                "B": {
                  "type": "string"
                },
                "C": {
                  "type": "string"
                }
              },
              "required": [
                "B",
                "C"
              ]
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
                "A": {
                  "type": "string"
                },
                "B": {
                  "type": "string"
                },
                "C": {
                  "type": "string"
                }
              },
              "required": [
                "B"
              ]
            }
            """);
    }

    [Fact]
    public void Unifies_types_in_additional_properties_schema()
    {
        const string source1 = """
            {
              "type": "object",
              "additionalProperties": {
                "type": "string"
              }
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "additionalProperties": {
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
              "type": "object",
              "additionalProperties": {
                "type": [
                  "number",
                  "string"
                ]
              }
            }
            """);
    }

    [Fact]
    public void Fails_on_conflicting_description_in_additional_properties_schema()
    {
        const string source1 = """
            {
              "type": "object",
              "additionalProperties": {
                "type": "string",
                "description": "One"
              }
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "additionalProperties": {
                "type": "number",
                "description": "Two"
              }
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);

        Action action = () => schemaMerger.AddSourceText(source2);

        action.Should().ThrowExactly<JSchemaException>().WithMessage("Conflict in schema at (3,27) in element 'description': 'Two' vs 'One'.");
    }

    [Theory]
    [InlineData("false")]
    [InlineData("{ \"type\": \"boolean\" }")]
    public void Preserves_additional_properties_constraint_when_same(string value)
    {
        string source = $$"""
            {
              "type": "object",
              "additionalProperties": {{value}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson(source);
    }

    [Fact]
    public void Removes_additional_properties_constraint_when_true()
    {
        const string source = """
            {
              "type": "object",
              "additionalProperties": true
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source);
        schemaMerger.AddSourceText(source);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object"
            }
            """);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("{ \"type\": \"boolean\" }")]
    public void Removes_additional_properties_constraint_when_missing(string value)
    {
        string source1 = $$"""
            {
              "type": "object",
              "additionalProperties": {{value}}
            }
            """;

        const string source2 = """
            {
              "type": "object"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object"
            }
            """);
    }

    [Fact]
    public void Prefers_additional_properties_schema_over_false()
    {
        const string source1 = """
            {
              "type": "object",
              "additionalProperties": false
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "additionalProperties": {
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
              "type": "object",
              "additionalProperties": {
                "type": "string"
              }
            }
            """);
    }

    [Fact]
    public void Prefers_additional_properties_true_over_schema()
    {
        const string source1 = """
            {
              "type": "object",
              "additionalProperties": true
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "additionalProperties": {
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
              "type": "object"
            }
            """);
    }

    [Fact]
    public void Prefers_omitted_additional_properties_over_schema()
    {
        const string source1 = """
            {
              "type": "object"
            }
            """;

        const string source2 = """
            {
              "type": "object",
              "additionalProperties": {
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
              "type": "object"
            }
            """);
    }

    [Fact]
    public void Prefers_omitted_additional_properties_over_false()
    {
        const string source1 = """
            {
              "type": "object",
              "additionalProperties": false
            }
            """;

        const string source2 = """
            {
              "type": "object"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object"
            }
            """);
    }

    [Theory]
    [InlineData("minProperties")]
    [InlineData("maxProperties")]
    public void Preserves_property_count_constraint_when_same(string keyword)
    {
        string source = $$"""
            {
              "type": "object",
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
    [InlineData("minProperties")]
    [InlineData("maxProperties")]
    public void Removes_property_count_constraint_when_missing(string keyword)
    {
        string source1 = $$"""
            {
              "type": "object",
              "{{keyword}}": 2
            }
            """;

        const string source2 = """
            {
              "type": "object"
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson("""
            {
              "type": "object"
            }
            """);
    }

    [Theory]
    [InlineData("minProperties", 2, 4, 2)]
    [InlineData("maxProperties", 2, 4, 4)]
    public void Unifies_property_count_constraint(string keyword, int value1, int value2, int resultValue)
    {
        string source1 = $$"""
            {
              "type": "object",
              "{{keyword}}": {{value1}}
            }
            """;

        string source2 = $$"""
            {
              "type": "object",
              "{{keyword}}": {{value2}}
            }
            """;

        var schemaMerger = new JsonSchemaMerger();
        schemaMerger.AddSourceText(source1);
        schemaMerger.AddSourceText(source2);

        string? resultJson = schemaMerger.GetResult();

        resultJson.Should().BeJson($$"""
            {
              "type": "object",
              "{{keyword}}": {{resultValue}}
            }
            """);
    }
}
