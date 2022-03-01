// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Binding
{
    internal static class TypeExtensions
    {
        internal static Type? GetElementTypeIfEnumerable(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type == typeof(string))
            {
                return null;
            }

            Type? enumerableInterface = null;

            if (type.IsEnumerable())
            {
                enumerableInterface = type;
            }

            return enumerableInterface?.GenericTypeArguments switch
            {
                { Length: 1 } genericTypeArguments => genericTypeArguments[0],
                _ => null
            };
        }

        internal static bool IsEnumerable(this Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            return
                type.IsArray
                ||
                typeof(IEnumerable).IsAssignableFrom(type);
        }

        internal static bool IsNullable(this Type t) => Nullable.GetUnderlyingType(t) is not null;

        internal static bool TryGetNullableType(
            this Type type,
            [NotNullWhen(true)] out Type? nullableType)
        {
            nullableType = Nullable.GetUnderlyingType(type);
            return nullableType is not null;
        }
    }
}