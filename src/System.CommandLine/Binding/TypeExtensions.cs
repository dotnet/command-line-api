// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Linq;

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

            Type enumerableInterface;

            if (type.IsEnumerable())
            {
                enumerableInterface = type;
            }
            else
            {
                enumerableInterface = type
                                      .GetInterfaces()
                                      .FirstOrDefault(IsEnumerable);
            }

            if (enumerableInterface is null)
            {
                return null;
            }

            return enumerableInterface.GenericTypeArguments switch
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

        internal static bool IsNullable(this Type t)
        {
            return t.IsGenericType &&
                   t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}