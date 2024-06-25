// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ConfigurationSchemaGenerator;

// External references for understanding the JSON schema keywords:
// - https://ajv.js.org/json-schema.html
// - https://github.com/json-schema-org/JSON-Schema-Test-Suite/blob/main/tests/draft2020-12/patternProperties.json

internal sealed class JsonSchemaMerger
{
    private static readonly JSchemaReaderSettings ReaderSettings = new()
    {
        ResolveSchemaReferences = false
    };

    private static readonly JsonTokenMerger TokenMerger = new();
    private static readonly JsonSchemaPropertySorter Sorter = new();

    private readonly JSchema _root = new();

    public void AddSourceFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        using StreamReader fileReader = File.OpenText(path);
        using var jsonReader = new JsonTextReader(fileReader);
        JSchema sourceSchema = JSchema.Load(jsonReader, ReaderSettings);

        MergeSchema(sourceSchema, _root);
    }

    public void AddSourceText(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        JSchema sourceSchema = JSchema.Parse(text, ReaderSettings);
        MergeSchema(sourceSchema, _root);
    }

    public string? GetResult()
    {
        return Sorter.Sort(_root);
    }

    private static void MergeSchema(JSchema source, JSchema target)
    {
        AssertNoCompoundConstructs(source);
        AssertNoReferenceConstructs(source);
        AssertNoUnsupportedObjectConstructs(source);
        AssertNoUnsupportedArrayConstructs(source);

        if (source.Type != null)
        {
            if (source.Type.Value.HasFlag(JSchemaType.Integer))
            {
                MergeNumericSchema(source, target, JSchemaType.Integer);
            }

            if (source.Type.Value.HasFlag(JSchemaType.Number))
            {
                MergeNumericSchema(source, target, JSchemaType.Number);
            }

            if (source.Type.Value.HasFlag(JSchemaType.String))
            {
                MergeStringSchema(source, target);
            }

            if (source.Type.Value.HasFlag(JSchemaType.Object))
            {
                MergeObjectSchema(source, target);
            }

            if (source.Type.Value.HasFlag(JSchemaType.Array))
            {
                MergeArraySchema(source, target);
            }

            // There's nothing specific to merge for schema types Boolean and Null.

            target.Type ??= source.Type;
            target.Type |= source.Type.Value;
        }

        MergeCommon(source, target);
    }

    private static void AssertNoCompoundConstructs(JSchema source)
    {
        if (source.Not != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, nameof(JSchema.Not));
        }

        if (source.OneOf.Count > 0)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, nameof(JSchema.OneOf));
        }

        if (source.AnyOf.Count > 0)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, nameof(JSchema.AnyOf));
        }

        if (source.AllOf.Count > 0)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, nameof(JSchema.AllOf));
        }

        if (source.If != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, nameof(JSchema.If));
        }

        if (source.Then != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, nameof(JSchema.Then));
        }

        if (source.Else != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, nameof(JSchema.Else));
        }
    }

    private static void AssertNoReferenceConstructs(JSchema source)
    {
        // Spec: "When $ref is used, all other keywords are ignored." We can't merge that.

        if (source.Ref != null || source.Reference != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, "$ref");
        }

        if (source.RecursiveReference != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, "$recursiveRef");
        }

        if (source.RecursiveAnchor != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, "$recursiveAnchor");
        }

        if (source.DynamicReference != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, "$dynamicRef");
        }

        if (source.Anchor != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, "$anchor");
        }

        if (source.Id != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(source, "$id");
        }
    }

    private static void AssertNoUnsupportedObjectConstructs(JSchema schema)
    {
        if (schema.PropertyNames != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(schema, nameof(JSchema.PropertyNames));
        }

        if (schema.Dependencies.Count > 0)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(schema, nameof(JSchema.Dependencies));
        }

        if (schema.DependentRequired.Count > 0)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(schema, nameof(JSchema.DependentRequired));
        }

        if (schema.DependentSchemas.Count > 0)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(schema, nameof(JSchema.DependentSchemas));
        }

        if (schema.AllowUnevaluatedProperties != null || schema.UnevaluatedProperties != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(schema, nameof(JSchema.UnevaluatedProperties));
        }
    }

    private static void AssertNoUnsupportedArrayConstructs(JSchema schema)
    {
        if (schema.AllowUnevaluatedItems != null || schema.UnevaluatedItems != null)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(schema, nameof(JSchema.UnevaluatedItems));
        }

        if (schema.Items.Count > 1 || schema.ItemsPositionValidation)
        {
            throw ErrorFactory.GetErrorForUnsupportedSchema(schema, "items (with tuple syntax)");
        }
    }

    private static void MergeNumericSchema(JSchema source, JSchema target, JSchemaType schemaType)
    {
        if (target.Type == null || !target.Type.Value.HasFlag(schemaType))
        {
            target.MultipleOf = source.MultipleOf;
            target.Minimum = source.Minimum;
            target.ExclusiveMinimum = source.ExclusiveMinimum;
            target.Maximum = source.Maximum;
            target.ExclusiveMaximum = source.ExclusiveMaximum;
        }
        else
        {
            if (!Equals(source.MultipleOf, target.MultipleOf))
            {
                target.MultipleOf = null;
            }

            if (!Equals(source.Minimum, target.Minimum) || source.ExclusiveMinimum != target.ExclusiveMinimum)
            {
                target.Minimum = null;
            }

            if (!Equals(source.Maximum, target.Maximum) || source.ExclusiveMaximum != target.ExclusiveMaximum)
            {
                target.Maximum = null;
            }
        }
    }

    private static void MergeStringSchema(JSchema source, JSchema target)
    {
        if (target.Type == null || !target.Type.Value.HasFlag(JSchemaType.String))
        {
            target.Pattern = source.Pattern;
            target.Format = source.Format;
            target.MinimumLength = source.MinimumLength;
            target.MaximumLength = source.MaximumLength;
        }
        else
        {
            if (source.Pattern != target.Pattern)
            {
                target.Pattern = null;
            }

            if (source.Format != target.Format)
            {
                target.Format = null;
            }

            if (source.MinimumLength != target.MinimumLength)
            {
                target.MinimumLength = null;
            }

            if (source.MaximumLength != target.MaximumLength)
            {
                target.MaximumLength = null;
            }
        }
    }

    private static void MergeObjectSchema(JSchema source, JSchema target)
    {
        if (target.Type == null || !target.Type.Value.HasFlag(JSchemaType.Object))
        {
            target.Properties.CopyFrom(source.Properties);
            target.PatternProperties.CopyFrom(source.PatternProperties);
            target.Required.CopyFrom(source.Required);
            target.AdditionalProperties = source.AdditionalProperties;
            target.AllowAdditionalProperties = source.AllowAdditionalProperties;
            target.AllowAdditionalPropertiesSpecified = source.AllowAdditionalPropertiesSpecified;
            target.MinimumProperties = source.MinimumProperties;
            target.MaximumProperties = source.MaximumProperties;
        }
        else
        {
            UnionStringToSchemaDictionary(source.Properties, target.Properties);
            UnionStringToSchemaDictionary(source.PatternProperties, target.PatternProperties);
            IntersectStringList(source.Required, target.Required);

            if (source.AdditionalProperties != null && target.AdditionalProperties != null)
            {
                // Both are sub-schemas, merge
                MergeSchema(source.AdditionalProperties, target.AdditionalProperties);
            }
            else
            {
                bool? sourceAllowAdditionalProperties = source.AllowAdditionalPropertiesSpecified ? source.AllowAdditionalProperties :
                    source.AdditionalProperties == null ? true : null;

                bool? targetAllowAdditionalProperties = target.AllowAdditionalPropertiesSpecified ? target.AllowAdditionalProperties :
                    target.AdditionalProperties == null ? true : null;

                if (sourceAllowAdditionalProperties == true || targetAllowAdditionalProperties == true)
                {
                    // allow = true (same as omitted) always wins
                    target.AdditionalProperties = null;
                    target.AllowAdditionalPropertiesSpecified = false;
                }
                else if (sourceAllowAdditionalProperties == false && targetAllowAdditionalProperties == false)
                {
                    // both are allow = false, we're done
                }
                else
                {
                    // one of them is a sub-schema, which wins over allow = false
                    if (source.AdditionalProperties != null)
                    {
                        target.AdditionalProperties = source.AdditionalProperties;
                        target.AllowAdditionalPropertiesSpecified = false;
                    }
                }
            }

            target.MinimumProperties = UnifyMinimumConstraint(source.MinimumProperties, target.MinimumProperties);
            target.MaximumProperties = UnifyMaximumConstraint(source.MaximumProperties, target.MaximumProperties);
        }
    }

    private static void MergeArraySchema(JSchema source, JSchema target)
    {
        if (target.Type == null || !target.Type.Value.HasFlag(JSchemaType.Array))
        {
            target.Items.CopyFrom(source.Items);
            target.AdditionalItems = source.AdditionalItems;
            target.AllowAdditionalItems = source.AllowAdditionalItems;
            target.AllowAdditionalItemsSpecified = source.AllowAdditionalItemsSpecified;
            target.Contains = source.Contains;
            target.MinimumContains = source.MinimumContains;
            target.MaximumContains = source.MaximumContains;
            target.MinimumItems = source.MinimumItems;
            target.MaximumItems = source.MaximumItems;
            target.UniqueItems = source.UniqueItems;
        }
        else
        {
            if (source.Items.Count == 1 && target.Items.Count == 1)
            {
                MergeSchema(source.Items[0], target.Items[0]);
            }
            else if (target.Items.Count == 1 && source.Items.Count == 0)
            {
                target.Items.Clear();
            }

            if (source.AdditionalItems != null && target.AdditionalItems != null)
            {
                // Both are sub-schemas, merge
                MergeSchema(source.AdditionalItems, target.AdditionalItems);
            }
            else
            {
                bool? sourceAllowAdditionalItems = source.AllowAdditionalItemsSpecified ? source.AllowAdditionalItems :
                    source.AdditionalItems == null ? true : null;

                bool? targetAllowAdditionalItems = target.AllowAdditionalItemsSpecified ? target.AllowAdditionalItems :
                    target.AdditionalItems == null ? true : null;

                if (sourceAllowAdditionalItems == true || targetAllowAdditionalItems == true)
                {
                    // allow = true (same as omitted) always wins
                    target.AdditionalItems = null;
                    target.AllowAdditionalItemsSpecified = false;
                }
                else if (sourceAllowAdditionalItems == false && targetAllowAdditionalItems == false)
                {
                    // both are allow = false, we're done
                }
                else
                {
                    // one of them is a sub-schema, which wins over allow = false
                    if (source.AdditionalItems != null)
                    {
                        target.AdditionalItems = source.AdditionalItems;
                        target.AllowAdditionalItemsSpecified = false;
                    }
                }
            }

            if (source.Contains != null && target.Contains != null)
            {
                MergeSchema(source.Contains, target.Contains);
            }
            else if (source.Contains == null && target.Contains != null)
            {
                target.Contains = null;
            }

            target.MinimumContains = UnifyMinimumConstraint(source.MinimumContains, target.MinimumContains);
            target.MaximumContains = UnifyMaximumConstraint(source.MaximumContains, target.MaximumContains);

            target.MinimumItems = UnifyMinimumConstraint(source.MinimumItems, target.MinimumItems);
            target.MaximumItems = UnifyMaximumConstraint(source.MaximumItems, target.MaximumItems);

            if (source.UniqueItems != target.UniqueItems)
            {
                target.UniqueItems = false;
            }
        }
    }

    private static void MergeCommon(JSchema source, JSchema target)
    {
        target.Const = MergeSchemaValue(source, source.Const, target.Const, nameof(JSchema.Const), JToken.EqualityComparer);
        UnionList(source.Enum, target.Enum, JToken.EqualityComparer);

        MergeMetadata(source, target);
        MergeExtensionData(source, target);
    }

    private static void MergeMetadata(JSchema source, JSchema target)
    {
        target.Title = MergeSchemaValue(source, source.Title, target.Title, nameof(JSchema.Title), StringComparer.Ordinal);
        target.Description = MergeSchemaValue(source, source.Description, target.Description, nameof(JSchema.Description), StringComparer.Ordinal);
        target.Default = MergeSchemaValue(source, source.Default, target.Default, nameof(JSchema.Default), JToken.EqualityComparer);
        target.SchemaVersion = MergeSchemaValue(source, source.SchemaVersion, target.SchemaVersion, "$schema", UriStrictEqualityComparer.Instance);

        target.ReadOnly = (source.ReadOnly ?? false) != (target.ReadOnly ?? false) ? null : source.ReadOnly;
        target.WriteOnly = (source.WriteOnly ?? false) != (target.WriteOnly ?? false) ? null : source.WriteOnly;

        if (target is { ReadOnly: true, WriteOnly: true })
        {
            target.ReadOnly = null;
            target.WriteOnly = null;
        }

        target.ContentEncoding = MergeSchemaValue(source, source.ContentEncoding, target.ContentEncoding, nameof(JSchema.ContentEncoding),
            StringComparer.Ordinal);

        target.ContentMediaType = MergeSchemaValue(source, source.ContentMediaType, target.ContentMediaType, nameof(JSchema.ContentMediaType),
            StringComparer.Ordinal);
    }

    private static void MergeExtensionData(JSchema source, JSchema target)
    {
        // JSchema can't parse the logging levels in "definitions", because they reference "#/definitions/logLevelThreshold", which isn't provided.

        if (source.ExtensionData.Count > 0)
        {
            foreach ((string sourceKey, JToken sourceToken) in source.ExtensionData)
            {
                if (!target.ExtensionData.TryGetValue(sourceKey, out JToken? targetToken))
                {
                    target.ExtensionData[sourceKey] = sourceToken;
                }
                else
                {
                    target.ExtensionData[sourceKey] = TokenMerger.Merge(sourceToken, targetToken);
                }
            }
        }
    }

    private static long? UnifyMinimumConstraint(long? sourceValue, long? targetValue)
    {
        if (sourceValue != targetValue)
        {
            if (sourceValue == null)
            {
                return null;
            }

            if (targetValue != null)
            {
                return Math.Min(sourceValue.Value, targetValue.Value);
            }
        }

        return targetValue;
    }

    private static long? UnifyMaximumConstraint(long? sourceValue, long? targetValue)
    {
        if (sourceValue != targetValue)
        {
            if (sourceValue == null)
            {
                return null;
            }

            if (targetValue != null)
            {
                return Math.Max(sourceValue.Value, targetValue.Value);
            }
        }

        return targetValue;
    }

    private static void UnionStringToSchemaDictionary(IDictionary<string, JSchema> source, IDictionary<string, JSchema> target)
    {
        foreach ((string sourceKey, JSchema sourceValue) in source)
        {
            if (!target.TryAdd(sourceKey, sourceValue))
            {
                JSchema targetValue = target[sourceKey];
                MergeSchema(sourceValue, targetValue);
            }
        }
    }

    private static void IntersectStringList(IList<string> source, IList<string> target)
    {
        List<string> itemsToRemove = target.Where(item => !source.Contains(item)).ToList();

        foreach (string key in itemsToRemove)
        {
            target.Remove(key);
        }
    }

    private static void UnionList<T>(IList<T> source, IList<T> target, IEqualityComparer<T> comparer)
    {
        foreach (T item in source.Where(item => !target.Contains(item, comparer)))
        {
            target.Add(item);
        }
    }

    private static T? MergeSchemaValue<T>(JSchema source, T? sourceValue, T? targetValue, string name, IEqualityComparer<T> comparer)
        where T : class
    {
        if (sourceValue != null && targetValue != null)
        {
            if (!comparer.Equals(sourceValue, targetValue))
            {
                throw ErrorFactory.GetErrorForSchemaConflict(source, sourceValue, targetValue, name);
            }
        }

        return sourceValue ?? targetValue;
    }
}
