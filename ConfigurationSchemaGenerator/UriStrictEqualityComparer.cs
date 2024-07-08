// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

namespace ConfigurationSchemaGenerator;

internal sealed class UriStrictEqualityComparer : IEqualityComparer<Uri>
{
    public static UriStrictEqualityComparer Instance { get; } = new();

    private UriStrictEqualityComparer()
    {
    }

    public bool Equals(Uri? x, Uri? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.AbsoluteUri == y.AbsoluteUri;
    }

    public int GetHashCode(Uri obj)
    {
        return obj.AbsoluteUri.GetHashCode();
    }
}
