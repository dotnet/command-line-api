// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    internal static class Binder
    {
        internal static bool CanBeBoundFromScalarValue(this Type type)
        {
            while (true)
            {
                if (type.IsPrimitive || type.IsEnum)
                {
                    return true;
                }

                if (type == typeof(string))
                {
                    return true;
                }

                if (TypeDescriptor.GetConverter(type) is { } typeConverter && 
                    typeConverter.CanConvertFrom(typeof(string)))
                {
                    return true;
                }

                if (TryFindConstructorWithSingleParameterOfType(type, typeof(string), out _))
                {
                    return true;
                }

                if (GetItemTypeIfEnumerable(type) is { } itemType)
                {
                    type = itemType;
                    continue;
                }

                return false;
            }
        }

        private static MethodInfo EnumerableEmptyMethod { get; }
            = typeof(Enumerable).GetMethod(nameof(Array.Empty));

        internal static object? GetDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return "";
            }

            if (GetItemTypeIfEnumerable(type) is { } itemType)
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
                var genericMethod = EnumerableEmptyMethod.MakeGenericMethod(itemType);
                return (IEnumerable)genericMethod.Invoke(null, new object[0]);
            }

            static Array CreateEmptyArray(Type itemType)
                => Array.CreateInstance(itemType, 0);
        }

        internal static Type? GetItemTypeIfEnumerable(Type type)
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

            for (var aliasIndex = IndexAfterPrefix(alias); 
                aliasIndex < alias.Length && parameterNameIndex < parameterName.Length;
                aliasIndex++)
            {
                var aliasChar = alias[aliasIndex];

                if (aliasChar == '-')
                {
                    continue;
                }

                var parameterNameChar = parameterName[parameterNameIndex];

                if (aliasChar == '|')
                {
                    parameterNameIndex += 2;
                    continue;
                }

                if (char.ToUpperInvariant(parameterNameChar) != char.ToUpperInvariant(aliasChar))
                {
                    return false;
                }

                parameterNameIndex++;
            }

            return parameterName.Length <= parameterNameIndex;

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

        internal static bool IsMatch(this string parameterName, IOption symbol) =>
            parameterName.IsMatch(symbol.Name) ||
            symbol.HasAlias(parameterName);

        internal static bool IsNullable(this Type t)
        {
            return t.IsGenericType &&
                   t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static bool TryFindConstructorWithSingleParameterOfType(
            this Type type,
            Type parameterType,
            [NotNullWhen(true)] out ConstructorInfo? ctor)
        {
            var (x, _) = type.GetConstructors()
                             .Select(c => (ctor: c, parameters: c.GetParameters()))
                             .SingleOrDefault(tuple => tuple.ctor.IsPublic &&
                                                       tuple.parameters.Length == 1 &&
                                                       tuple.parameters[0].ParameterType == parameterType);

            if (x != null)
            {
                ctor = x;
                return true;
            }
            else
            {
                ctor = null;
                return false;
            }
        }
    }
}
