// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Help
{
    internal static class HelpBuilderExtensions
    {
        internal static IEnumerable<Symbol> GetParameters(this Symbol symbol)
        {
            switch (symbol)
            {
                case Option option:
                    yield return option;
                    yield break;
                case Command command:
                    foreach (var argument in command.Arguments)
                    {
                        yield return argument;
                    }
                    yield break;
                case Argument argument:
                    yield return argument;
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }

        internal static (string? Prefix, string Alias) SplitPrefix(this string rawAlias)
        {
            if (rawAlias[0] == '/')
            {
                return ("/", rawAlias.Substring(1));
            }
            else if (rawAlias[0] == '-')
            {
                if (rawAlias.Length > 1 && rawAlias[1] == '-')
                {
                    return ("--", rawAlias.Substring(2));
                }

                return ("-", rawAlias.Substring(1));
            }

            return (null, rawAlias);
        }

        internal static IEnumerable<T> RecurseWhileNotNull<T>(this T? source, Func<T, T?> next) where T : class
        {
            while (source is not null)
            {
                yield return source;

                source = next(source);
            }
        }
    }
}