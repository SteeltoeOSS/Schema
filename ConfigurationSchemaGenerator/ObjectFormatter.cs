// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ConfigurationSchemaGenerator;

internal static class ObjectFormatter
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented
    };

    [return: NotNullIfNotNull("value")]
    public static string? FormatObject(object? value)
    {
        if (value is JSchema or JToken)
        {
            return JsonConvert.SerializeObject(value, JsonSerializerSettings);
        }

        if (value is IConvertible convertible)
        {
            return convertible.ToString(CultureInfo.InvariantCulture);
        }

        return value?.ToString();
    }
}
