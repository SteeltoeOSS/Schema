// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

namespace ConfigurationSchemaGenerator;

internal static class CollectionExtensions
{
    public static void CopyFrom<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        target.Clear();

        foreach ((TKey sourceKey, TValue sourceValue) in source)
        {
            target.Add(sourceKey, sourceValue);
        }
    }

    public static void CopyFrom<T>(this ICollection<T> target, ICollection<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        target.Clear();

        foreach (T sourceItem in source)
        {
            target.Add(sourceItem);
        }
    }
}
