// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Binding
{
    internal static class Binder
    {
        internal static bool IsMatch(this string parameterName, string alias)
        {
            var parameterIndex = 0;
            for (var aliasIndex = alias.AdvancePrefix(); aliasIndex < alias.Length; aliasIndex++)
            {
                var aliasChar = alias[aliasIndex];

                if (aliasChar == '-')
                {
                    continue;
                }

                var parameterNameChar = parameterName[parameterIndex];

                if (aliasChar == '|')
                {
                    if ((parameterName.Length < parameterIndex + 2) || ((parameterNameChar | 32) != 'o') || ((parameterName[parameterIndex + 1] | 32) != 'r'))
                    {
                        return false;
                    }

                    parameterIndex += 2;

                    continue;
                }

                if (parameterNameChar != aliasChar && char.ToUpperInvariant(parameterNameChar) != char.ToUpperInvariant(aliasChar))
                {
                    return false;
                }

                parameterIndex++;
            }

            return true;
        }

        internal static bool IsMatch(this string parameterName, IOption symbol) =>
            parameterName.IsMatch(symbol.Name) ||
            symbol.HasAlias(parameterName);

        internal static bool IsNullable(this Type t)
        {
            return t.IsGenericType &&
                   t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static object? GetDefaultValueForType(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
