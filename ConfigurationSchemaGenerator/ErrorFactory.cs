// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ConfigurationSchemaGenerator;

internal static class ErrorFactory
{
    public static JSchemaException GetErrorForSchemaConflict(JSchema source, object? sourceValue, object? targetValue, string name)
    {
        string camelCaseName = $"{char.ToLowerInvariant(name[0])}{name[1..]}";
        string? sourceValueText = ObjectFormatter.FormatObject(sourceValue);
        string? targetValueText = ObjectFormatter.FormatObject(targetValue);

        throw new JSchemaException($"Conflict in schema{GetSchemaPosition(source)} in element '{camelCaseName}': '{sourceValueText}' vs '{targetValueText}'.");
    }

    public static JSchemaException GetErrorForUnsupportedSchema(JSchema source, string name)
    {
        string camelCaseName = $"{char.ToLowerInvariant(name[0])}{name[1..]}";
        throw new JSchemaException($"Unsupported schema construct '{camelCaseName}'{GetSchemaPosition(source)}.");
    }

    public static Exception GetErrorForTokenConflict(JToken source, object? sourceValue, object? targetValue)
    {
        string? sourceValueText = ObjectFormatter.FormatObject(sourceValue);
        string? targetValueText = ObjectFormatter.FormatObject(targetValue);

        throw new JsonException($"Conflict in JSON node{GetNodePath(source)} within schema extension data: '{sourceValueText}' vs '{targetValueText}'.");
    }

    public static Exception GetErrorForTokenTypeConflict(JToken source, JToken target)
    {
        throw new JsonException($"Conflict in JSON node type{GetNodePath(source)} within schema extension data: '{source.Type}' vs '{target.Type}'.");
    }

    public static Exception GetErrorForUnsupportedToken(JToken source)
    {
        string typeText = ObjectFormatter.FormatObject(source.Type);
        throw new JsonException($"Unsupported JSON node type {typeText}{GetNodePath(source)} within schema extension data.");
    }

    private static string GetSchemaPosition(JSchema schema)
    {
        var lineInfo = (IJsonLineInfo)schema;

        if (lineInfo.HasLineInfo())
        {
            return $" at ({lineInfo.LineNumber},{lineInfo.LinePosition})";
        }

        return string.Empty;
    }

    private static string GetNodePath(JToken token)
    {
        if (!string.IsNullOrEmpty(token.Path))
        {
            return $" at path '{token.Path}'";
        }

        return string.Empty;
    }
}
