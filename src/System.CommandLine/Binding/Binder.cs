// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal static class Binder
    {
        internal static bool IsMatch(this string parameterName, string alias)
        {
            var parameterNameIndex = 0;
            var aliasIndex = IndexAfterPrefix(alias);

            while (aliasIndex < alias.Length && parameterNameIndex < parameterName.Length)
            {
                var aliasChar = alias[aliasIndex++];

                if (aliasChar == '-')
                {
                    continue;
                }

                var parameterNameChar = parameterName[parameterNameIndex];

                if (aliasChar == '|')
                {
                    if (parameterName.Length < parameterNameIndex + 2 || 
                        (parameterNameChar | 32) != 'o' || 
                        (parameterName[parameterNameIndex + 1] | 32) != 'r')
                    {
                        return false;
                    }

                    parameterNameIndex += 2;

                    continue;
                }

                if (parameterNameChar != aliasChar && 
                    char.ToUpperInvariant(parameterNameChar) != char.ToUpperInvariant(aliasChar))
                {
                    return false;
                }

                parameterNameIndex++;
            }

            return aliasIndex == alias.Length && parameterNameIndex == parameterName.Length;


            static int IndexAfterPrefix(string alias)
            {
                if (alias.Length > 0)
                {
                    if (alias[0] == '-' && alias.Length > 1 && alias[1] == '-')
                    {
                        return 2;
                    }
                    else if (alias[0] == '-')
                    {
                        return 1;
                    }
                    else if (alias[0] == '/')
                    {
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

        public static object? GetDefaultValueForType(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
