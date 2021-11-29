// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace System.CommandLine.Binding
{
    internal static class Binder
    {
        private static Lazy<MethodInfo> EnumerableEmptyMethod { get; } = new
             (() => typeof(Enumerable).GetMethod(nameof(Array.Empty)), LazyThreadSafetyMode.None);

        internal static object? GetDefaultValue(Type type)
        {
            if (GetElementTypeIfEnumerable(type) is { } itemType)
            {
                if (type.IsArray)
                {
                    return CreateEmptyArray(itemType);
                }

                if (type.IsGenericType)
                {
                    return type.GetGenericTypeDefinition() switch
                    {
                        { } enumerable when enumerable == typeof(IEnumerable<>) => GetEmptyEnumerable(itemType),
                        { } list when list == typeof(List<>) => GetEmptyList(itemType),
                        { } array when array == typeof(IList<>) || 
                                       array == typeof(ICollection<>) => CreateEmptyArray(itemType),
                        _ => null
                    };
                }
            }

            return type switch
            {
                { } nonGeneric 
                when nonGeneric == typeof(IList) ||
                     nonGeneric == typeof(ICollection) ||
                     nonGeneric == typeof(IEnumerable)
                => CreateEmptyArray(typeof(object)),
                _ => type.IsValueType ? Activator.CreateInstance(type) : null
            };
            
            static object GetEmptyList(Type itemType)
            {
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
            }

            static IEnumerable GetEmptyEnumerable(Type itemType)
            {
                var genericMethod = EnumerableEmptyMethod.Value.MakeGenericMethod(itemType);
                return (IEnumerable)genericMethod.Invoke(null, new object[0]);
            }

            static Array CreateEmptyArray(Type itemType)
                => Array.CreateInstance(itemType, 0);
        }

        internal static Type? GetElementTypeIfEnumerable(Type type)
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

        internal static bool IsMatch(this string parameterName, string alias)
        {
            var parameterNameIndex = 0;

            var indexAfterPrefix = IndexAfterPrefix(alias);
            var parameterCandidateLength = alias.Length - indexAfterPrefix;

            for (var aliasIndex = indexAfterPrefix; 
                aliasIndex < alias.Length && parameterNameIndex < parameterName.Length;
                aliasIndex++)
            {
                var aliasChar = alias[aliasIndex];

                if (aliasChar == '-')
                {
                    parameterCandidateLength--;
                    continue;
                }

                var parameterNameChar = parameterName[parameterNameIndex];

                if (aliasChar == '|')
                {
                    // replacing "|" with "or"
                    parameterNameIndex += 2;
                    parameterCandidateLength++; 
                    continue;
                }
                
                if (char.ToUpperInvariant(parameterNameChar) != char.ToUpperInvariant(aliasChar))
                {
                    return false;
                }

                parameterNameIndex++;
            }

            if (parameterCandidateLength == parameterName.Length)
            {
                return true;
            }

            return false;

            static int IndexAfterPrefix(string alias)
            {
                if (alias.Length > 0)
                {
                    switch (alias[0])
                    {
                        case '-' when alias.Length > 1 && alias[1] == '-':
                            return 2;
                        case '-':
                        case '/':
                            return 1;
                    }
                }

                return 0;
            }
        }

        internal static bool IsNullable(this Type t)
        {
            return t.IsGenericType &&
                   t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
