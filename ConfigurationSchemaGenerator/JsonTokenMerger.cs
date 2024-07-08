// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json.Linq;

namespace ConfigurationSchemaGenerator;

internal sealed class JsonTokenMerger
{
    public JToken Merge(JToken source, JToken target)
    {
        // JContainer.Merge() doesn't serve our needs. It overwrites instead of raising a conflict.
        return MergeToken(source, target);
    }

    private static JToken MergeToken(JToken source, JToken target)
    {
        if (target.Type != source.Type)
        {
            throw ErrorFactory.GetErrorForTokenTypeConflict(source, target);
        }

        return source.Type switch
        {
            JTokenType.Object => MergeObjectToken((JObject)source, (JObject)target),
            JTokenType.Array => MergeArrayToken((JArray)source, (JArray)target),
            JTokenType.Integer or JTokenType.Float or JTokenType.String or JTokenType.Boolean => MergeValueToken((JValue)source, (JValue)target),
            JTokenType.Null => target, // Nothing to merge.
            _ => throw ErrorFactory.GetErrorForUnsupportedToken(source)
        };
    }

    private static JObject MergeObjectToken(JObject source, JObject target)
    {
        foreach ((string sourceKey, JToken? sourceValue) in source)
        {
            if (!target.TryAdd(sourceKey, sourceValue))
            {
                JToken? targetValue = target[sourceKey];

                if (sourceValue == null || targetValue == null)
                {
                    throw new Exception("Internal error: this location is thought to be unreachable.");
                }

                target[sourceKey] = MergeToken(sourceValue, targetValue);
            }
        }

        return target;
    }

    private static JArray MergeArrayToken(JArray source, JArray target)
    {
        foreach (JToken item in source.Where(item => !target.Contains(item, JToken.EqualityComparer)))
        {
            target.Add(item);
        }

        return target;
    }

    private static JValue MergeValueToken(JValue source, JValue target)
    {
        if (!Equals(source.Value, target.Value))
        {
            throw ErrorFactory.GetErrorForTokenConflict(source, source.Value, target.Value);
        }

        return target;
    }
}
