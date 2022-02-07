// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

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

        internal static bool IsNullable(this Type t)
        {
            return t.IsGenericType &&
                   t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        
        internal static bool TryGetNullableType(this Type type, out Type nullableType)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    nullableType = type.GetGenericArguments()[0];
                    return true;
                }
            }

            nullableType = null;
            return false;
        }
    }
}