// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ConfigurationSchemaGenerator;

internal sealed class JsonSchemaPropertySorter
{
    public string? Sort(JSchema schema)
    {
        string json = ObjectFormatter.FormatObject(schema);

        var schemaObject = (JObject?)JsonConvert.DeserializeObject(json);

        if (schemaObject != null)
        {
            SortSchema(schemaObject);
        }

        return ObjectFormatter.FormatObject(schemaObject);
    }

    private static void SortSchema(JObject schemaObject)
    {
        SortArrayProperty(schemaObject, "type");
        SortArrayProperty(schemaObject, "required");

        SortCollectionProperty(schemaObject, "properties");
        SortCollectionProperty(schemaObject, "patternProperties");

        SortCollectionContainer(schemaObject, "items");
        SortCollectionContainer(schemaObject, "contains");
        SortCollectionContainer(schemaObject, "additionalItems");
        SortCollectionContainer(schemaObject, "additionalProperties");

        MovePropertyToTop(schemaObject, "type");
        MovePropertyToTop(schemaObject, "definitions");
        MovePropertyToBottom(schemaObject, "summary");
        MovePropertyToBottom(schemaObject, "description");
    }

    private static void SortArrayProperty(JObject schemaObject, string propertyName)
    {
        JProperty? typeProperty = schemaObject.Property(propertyName);

        if (typeProperty is { Value: JArray array })
        {
            SortJsonStringArray(array);
        }
    }

    private static void SortJsonStringArray(JArray array)
    {
        List<JToken> elements = array.ToList();

        foreach (JToken element in elements)
        {
            element.Remove();
        }

        JToken[] elementsInOrder = elements.OrderBy(p => p is JValue { Value: string stringValue } ? stringValue : null, StringComparer.Ordinal).ToArray();

        foreach (JToken element in elementsInOrder)
        {
            array.Add(element);
        }
    }

    private static void SortCollectionProperty(JObject schemaObject, string propertyName)
    {
        JProperty? collectionProperty = schemaObject.Property(propertyName);

        if (collectionProperty is { Count: > 0 } and { Value: JObject propertyValue })
        {
            SortProperties(propertyValue);
        }
    }

    private static void SortCollectionContainer(JObject schemaObject, string containerName)
    {
        JProperty? containerProperty = schemaObject.Property(containerName);

        if (containerProperty is { Count: > 0 } and { Value: JObject propertyValue })
        {
            SortSchema(propertyValue);
        }
    }

    private static void SortProperties(JObject schemaObject)
    {
        List<JProperty> properties = schemaObject.Properties().ToList();

        foreach (JProperty property in properties)
        {
            property.Remove();
        }

        JProperty[] propertiesInOrder = properties.OrderBy(p => p.Name, StringComparer.Ordinal).ToArray();

        foreach (JProperty property in propertiesInOrder)
        {
            schemaObject.Add(property);

            if (property.Value is JObject innerObject)
            {
                SortSchema(innerObject);
            }
        }
    }

    private static void MovePropertyToTop(JObject schemaObject, string propertyName)
    {
        JProperty? property = schemaObject.Property(propertyName);

        if (property != null)
        {
            JProperty[] otherProperties = schemaObject.Properties().Where(nextProperty => nextProperty != property).ToArray();

            foreach (JProperty otherProperty in otherProperties)
            {
                schemaObject.Remove(otherProperty.Name);
            }

            foreach (JProperty otherProperty in otherProperties)
            {
                schemaObject.Add(otherProperty.Name, otherProperty.Value);
            }
        }
    }

    private static void MovePropertyToBottom(JObject schemaObject, string propertyName)
    {
        JProperty? property = schemaObject.Property(propertyName);

        if (property != null)
        {
            schemaObject.Remove(propertyName);
            schemaObject.Add(property);
        }
    }
}
