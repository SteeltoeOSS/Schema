// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using ConfigurationSchemaGenerator;
using Newtonsoft.Json.Schema;

namespace ConfigurationSchemaGeneratorTest;

public sealed partial class JsonSchemaMergerTests
{
    [Theory]
    // Compound
    [InlineData("not", """
        {
            "type": "object",
            "properties": {
                "x": { "type": "integer" }
            },
            "required": [ "x" ],
            "not": { "required": [ "z" ] }
        }
        """)]
    [InlineData("oneOf", """
        {
          "oneOf": [
            { "type": "number", "multipleOf": 5 },
            { "type": "number", "multipleOf": 3 }
          ]
        }
        """)]
    [InlineData("anyOf", """
        {
          "anyOf": [
            { "type": "string", "maxLength": 5 },
            { "type": "number", "minimum": 0 }
          ]
        }
        """)]
    [InlineData("allOf", """
        {
          "allOf": [
            { "type": "string" },
            { "maxLength": 5 }
          ]
        }
        """)]
    [InlineData("if", """
        {
          "type": "object",
          "properties": {
            "street_address": {
              "type": "string"
            },
            "country": {
              "default": "United States of America",
              "enum": ["United States of America", "Canada"]
            }
          },
          "if": {
            "properties": {
              "country": { "const": "United States of America" }
            }
          },
          "then": {
            "properties": {
              "postal_code": { "pattern": "[0-9]{5}(-[0-9]{4})?" }
            }
          },
          "else": {
            "properties": {
              "postal_code": { "pattern": "[A-Z][0-9][A-Z] [0-9][A-Z][0-9]" }
            }
          }
        }
        """)]
    [InlineData("then", """
        {
          "type": "object",
          "properties": {
            "street_address": {
              "type": "string"
            },
            "country": {
              "default": "United States of America",
              "enum": ["United States of America", "Canada"]
            }
          },
          "then": {
            "properties": {
              "postal_code": { "pattern": "[0-9]{5}(-[0-9]{4})?" }
            }
          },
          "else": {
            "properties": {
              "postal_code": { "pattern": "[A-Z][0-9][A-Z] [0-9][A-Z][0-9]" }
            }
          }
        }
        """)]
    [InlineData("else", """
        {
          "type": "object",
          "properties": {
            "street_address": {
              "type": "string"
            },
            "country": {
              "default": "United States of America",
              "enum": ["United States of America", "Canada"]
            }
          },
          "else": {
            "properties": {
              "postal_code": { "pattern": "[A-Z][0-9][A-Z] [0-9][A-Z][0-9]" }
            }
          }
        }
        """)]
    // References
    [InlineData("$ref", """
        {
          "$ref": "http://example.com/schema#"
        }
        """)]
    [InlineData("$recursiveRef", """
        {
          "$recursiveRef": "#"
        }
        """)]
    [InlineData("$recursiveAnchor", """
        {
          "$recursiveAnchor": true
        }
        """)]
    [InlineData("$dynamicRef", """
        {
          "$dynamicRef": "#node"
        }
        """)]
    [InlineData("$anchor", """
        {
          "$anchor": "example-anchor"
        }
        """)]
    [InlineData("$id", """
        {
          "$id": "http://example.com/schema#"
        }
        """)]
    // Objects
    [InlineData("propertyNames", """
        {
          "type": "object",
          "propertyNames": {
            "pattern": "^[A-Z]+$"
          }
        }
        """)]
    [InlineData("dependencies", """
        {
          "type": "object",
          "dependencies": {
            "a": ["b", "c"],
            "c": {
              "type": "object",
              "properties": {
                "b": {
                  "type": "integer"
                }
              }     
            }
          }
        }
        """)]
    [InlineData("dependentRequired", """
        {
          "type": "object",
          "properties": {
            "name": { "type": "string" },
            "credit_card": { "type": "number" },
            "billing_address": { "type": "string" }
          },
          "required": ["name"],
          "dependentRequired": {
            "credit_card": ["billing_address"]
          }
        }
        """)]
    [InlineData("dependentSchemas", """
        {
          "type": "object",
          "dependentSchemas": {
            "c": {
              "type": "object",
              "properties": {
                "b": {
                  "type": "integer"
                }
              }     
            }
          }
        }
        """)]
    [InlineData("unevaluatedProperties", """
        {
          "type": "object",
          "properties": {
            "type": { "enum": ["residential", "business"] }
          },
          "required": ["type"],
          "unevaluatedProperties": false
        }
        """)]
    [InlineData("unevaluatedProperties", """
        {
          "type": "object",
          "properties": {
            "type": { "enum": ["residential", "business"] }
          },
          "required": ["type"],
          "unevaluatedProperties": { "type": "string" }
        }
        """)]
    // Arrays
    [InlineData("unevaluatedItems", """
        {
          "prefixItems": [
            { "type": "string" }, { "type": "number" }
          ],
          "unevaluatedItems": false
        }
        """)]
    [InlineData("unevaluatedItems", """
        {
          "prefixItems": [{ "type": "boolean" }, { "type": "string" }],
          "unevaluatedItems": { "const": 2 }
        }
        """)]
    [InlineData("items (with tuple syntax)", """
        {
          "items": [
            { "type": "string" }, { "type": "number" }
          ]
        }
        """)]
    [InlineData("items (with tuple syntax)", """
        {
          "items": [
            { "type": "string" }
          ]
        }
        """)]
    public void Fails_on_unsupported_keyword(string keyword, string source)
    {
        var schemaMerger = new JsonSchemaMerger();

        Action action = () => schemaMerger.AddSourceText(source);

        action.Should().ThrowExactly<JSchemaException>().WithMessage($"Unsupported schema construct '{keyword}' at (1,1).");
    }
}
